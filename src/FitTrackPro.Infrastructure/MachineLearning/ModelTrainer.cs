namespace FitTrackPro.Infrastructure.MachineLearning;

using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Infrastructure.MachineLearning.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Vision;

/// <summary>
/// Dùng để train / retrain model từ dataset ảnh.
/// Chạy qua CLI hoặc Admin endpoint.
///
/// Cấu trúc thư mục training data:
///   scripts/training-data/
///     pho_bo/        ← 60+ ảnh jpg/png
///       001.jpg
///       002.jpg
///     com_tam/
///       001.jpg
///     banh_mi/
///       ...
/// </summary>
public class ModelTrainer : IModelTrainer
{
    private static readonly SemaphoreSlim TrainingGate = new(1, 1);
    private static readonly HttpClient ResourceHttpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(10)
    };

    private const string MlNetResourcePathEnvKey = "MICROSOFTML_RESOURCE_PATH";
    private const ImageClassificationTrainer.Architecture TrainerArchitecture =
        ImageClassificationTrainer.Architecture.ResnetV2101;

    private readonly MLContext _mlContext;
    private readonly FoodRecognitionOptions _options;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<ModelTrainer> _logger;

    public ModelTrainer(
        IOptions<FoodRecognitionOptions> options,
        IHostEnvironment hostEnvironment,
        ILogger<ModelTrainer> logger)
    {
        _mlContext = new MLContext(seed: 42);
        _options = options.Value;
        _hostEnvironment = hostEnvironment;
        _logger = logger;

        EnsureMlNetResourcePath();
    }

    // ─────────────────────────────────────────────────────
    // TRAIN & SAVE MODEL
    // ─────────────────────────────────────────────────────
    public async Task<TrainingResult> TrainAndSaveAsync(CancellationToken cancellationToken = default)
    {
        if (!await TrainingGate.WaitAsync(TimeSpan.Zero, cancellationToken))
            throw new InvalidOperationException("Đang có một tiến trình training khác chạy. Vui lòng thử lại sau.");

        try
        {
            _logger.LogInformation("[ModelTrainer] Bắt đầu quá trình training...");

            // 1. Kiểm tra thư mục data
            var trainingDataPath = ResolveTrainingDataPath();
            if (trainingDataPath is null)
            {
                var attemptedPaths = string.Join(", ", BuildPathCandidates(_options.TrainingDataPath));
                throw new DirectoryNotFoundException(
                    $"Không tìm thấy thư mục training data: {_options.TrainingDataPath}. Đã thử các đường dẫn: {attemptedPaths}");
            }

            _logger.LogInformation("[ModelTrainer] Đường dẫn training data: {Path}", trainingDataPath);

            // 2. Load data từ các subfolders (mỗi folder = 1 label)
            var imageData = LoadImagesFromFolders(trainingDataPath);
            _logger.LogInformation("[ModelTrainer] Tổng ảnh: {Count}", imageData.Count);

            if (imageData.Count < 20)
                throw new InvalidOperationException(
                    "Cần ít nhất 20 ảnh để train. Thêm ảnh vào thư mục training-data/.");

            // 3. Tạo IDataView
            var dataView = _mlContext.Data.LoadFromEnumerable(imageData);

            // 4. Shuffle và split 80/20
            var shuffled = _mlContext.Data.ShuffleRows(dataView);
            var split = _mlContext.Data.TrainTestSplit(shuffled, testFraction: 0.2);

            // 5. Warm up transfer-learning resources để tránh lỗi downloader nội bộ của ML.NET
            await EnsureTransferLearningResourcesAsync(cancellationToken);

            // 6. Build training pipeline (Transfer Learning với ResNet)
            var pipeline = BuildPipeline();

            // 7. Train (async để không block thread pool)
            _logger.LogInformation("[ModelTrainer] Đang train... (có thể mất vài phút)");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var model = await Task.Run(() => pipeline.Fit(split.TrainSet), cancellationToken);

            stopwatch.Stop();
            _logger.LogInformation("[ModelTrainer] Train xong sau {Seconds}s", stopwatch.Elapsed.TotalSeconds);

            // 8. Evaluate trên test set
            var predictions = model.Transform(split.TestSet);
            var metrics = _mlContext.MulticlassClassification.Evaluate(
                predictions,
                labelColumnName: "LabelKey",
                scoreColumnName: "Score",
                predictedLabelColumnName: "PredictedLabel");

            var result = new TrainingResult
            {
                MicroAccuracy = metrics.MicroAccuracy,
                MacroAccuracy = metrics.MacroAccuracy,
                LogLoss = metrics.LogLoss,
                TrainedAt = DateTime.UtcNow,
                TotalImages = imageData.Count,
                Categories = imageData.Select(i => i.Label).Distinct().Count(),
                ElapsedSeconds = stopwatch.Elapsed.TotalSeconds,
            };

            _logger.LogInformation(
                "[ModelTrainer] Kết quả: MicroAccuracy={MicroAcc:F3}, MacroAccuracy={MacroAcc:F3}",
                result.MicroAccuracy, result.MacroAccuracy);

            // 9. Save model
            var modelPath = ResolveModelOutputPath();
            EnsureModelDirectory(modelPath);
            _mlContext.Model.Save(model, dataView.Schema, modelPath);
            _logger.LogInformation(
                "[ModelTrainer] Model đã lưu tại: {Path} (configured: {ConfiguredPath})",
                modelPath,
                _options.ModelPath);

            return result;
        }
        finally
        {
            TrainingGate.Release();
        }
    }

    // ─────────────────────────────────────────────────────
    // PRIVATE: Build ML Pipeline
    // ─────────────────────────────────────────────────────
    private IEstimator<ITransformer> BuildPipeline()
    {
        // Bước 1: Map string label → key (yêu cầu của ImageClassification)
        var pipeline = _mlContext.Transforms.Conversion
            .MapValueToKey("LabelKey", nameof(FoodImageData.Label))

        // Bước 2: Load ảnh từ file path
        .Append(_mlContext.Transforms.LoadRawImageBytes(
            outputColumnName: "Image",
            imageFolder: null,
            inputColumnName: nameof(FoodImageData.ImagePath)))

        // Bước 3: ImageClassification Trainer (Transfer Learning)
        // Architecture mặc định: InceptionV3
        // Nếu muốn ResNet: thay Architecture = ImageClassificationTrainer.Architecture.ResnetV2101
        .Append(_mlContext.MulticlassClassification.Trainers.ImageClassification(
            new ImageClassificationTrainer.Options
            {
                LabelColumnName      = "LabelKey",
                FeatureColumnName    = "Image",
                Arch                 = TrainerArchitecture,
                Epoch                = _options.Epochs,
                BatchSize            = _options.BatchSize,
                LearningRate         = 0.01f,
                ValidationSet        = null, // dùng split ở trên
                ReuseTrainSetBottleneckCachedValues = true,
                MetricsCallback      = (metrics) =>
                {
                    var trainMetrics = metrics?.Train;
                    if (trainMetrics is null)
                        return;

                    if (trainMetrics.BatchProcessedCount % 10 == 0)
                        _logger.LogDebug("[ModelTrainer] Batch {Batch}: Accuracy={Acc:F3}",
                            trainMetrics.BatchProcessedCount, trainMetrics.Accuracy);
                }
            }))

        // Bước 4: Map key → label text ngược lại
        .Append(_mlContext.Transforms.Conversion
            .MapKeyToValue("PredictedLabel", "PredictedLabel"));

        return pipeline;
    }

    // ─────────────────────────────────────────────────────
    // PRIVATE: Load ảnh từ folder structure
    // ─────────────────────────────────────────────────────
    private static List<FoodImageData> LoadImagesFromFolders(string rootPath)
    {
        var result = new List<FoodImageData>();
        var supportedExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".bmp" };

        foreach (var categoryDir in Directory.GetDirectories(rootPath))
        {
            var label = Path.GetFileName(categoryDir); // tên folder = label

            var images = Directory
                .GetFiles(categoryDir, "*", SearchOption.TopDirectoryOnly)
                .Where(f => supportedExts.Contains(Path.GetExtension(f)))
                .ToList();

            foreach (var imgPath in images)
            {
                result.Add(new FoodImageData
                {
                    ImagePath = imgPath,
                    Label     = label
                });
            }
        }

        return result;
    }

    private static void EnsureModelDirectory(string modelPath)
    {
        var dir = Path.GetDirectoryName(modelPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }

    private string ResolveModelOutputPath()
    {
        if (Path.IsPathRooted(_options.ModelPath))
            return Path.GetFullPath(_options.ModelPath);

        return Path.GetFullPath(Path.Combine(_hostEnvironment.ContentRootPath, _options.ModelPath));
    }

    private string? ResolveTrainingDataPath()
    {
        foreach (var candidate in BuildPathCandidates(_options.TrainingDataPath))
        {
            if (Directory.Exists(candidate))
                return candidate;
        }

        return null;
    }

    private IEnumerable<string> BuildPathCandidates(string configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
            yield break;

        if (Path.IsPathRooted(configuredPath))
        {
            yield return Path.GetFullPath(configuredPath);
            yield break;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var basePath in GetBasePaths())
        {
            var candidate = Path.GetFullPath(Path.Combine(basePath, configuredPath));
            if (seen.Add(candidate))
                yield return candidate;
        }

        var direct = Path.GetFullPath(configuredPath);
        if (seen.Add(direct))
            yield return direct;
    }

    private IEnumerable<string> GetBasePaths()
    {
        yield return _hostEnvironment.ContentRootPath;
        yield return Directory.GetCurrentDirectory();
        yield return AppContext.BaseDirectory;

        var solutionRootFromContent = FindSolutionRoot(_hostEnvironment.ContentRootPath);
        if (!string.IsNullOrWhiteSpace(solutionRootFromContent))
            yield return solutionRootFromContent;

        var solutionRootFromCurrent = FindSolutionRoot(Directory.GetCurrentDirectory());
        if (!string.IsNullOrWhiteSpace(solutionRootFromCurrent))
            yield return solutionRootFromCurrent;
    }

    private static string? FindSolutionRoot(string startPath)
    {
        var directory = new DirectoryInfo(startPath);
        while (directory is not null)
        {
            if (directory.GetFiles("*.sln", SearchOption.TopDirectoryOnly).Length > 0)
                return directory.FullName;

            directory = directory.Parent;
        }

        return null;
    }

    private void EnsureMlNetResourcePath()
    {
        var configuredPath = Environment.GetEnvironmentVariable(MlNetResourcePathEnvKey);
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            Directory.CreateDirectory(configuredPath);
            return;
        }

        var tempRoot = Path.GetTempPath();
        if (string.IsNullOrWhiteSpace(tempRoot))
            tempRoot = _hostEnvironment.ContentRootPath;

        var fallbackPath = Path.GetFullPath(Path.Combine(tempRoot, "MLNET"));
        Directory.CreateDirectory(fallbackPath);
        Environment.SetEnvironmentVariable(MlNetResourcePathEnvKey, fallbackPath);

        _logger.LogInformation(
            "[ModelTrainer] Set {Key}={Path}",
            MlNetResourcePathEnvKey,
            fallbackPath);
    }

    private string GetMlNetResourcePath()
    {
        var path = Environment.GetEnvironmentVariable(MlNetResourcePathEnvKey);
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidOperationException($"{MlNetResourcePathEnvKey} chưa được thiết lập.");

        return path;
    }

    private async Task EnsureTransferLearningResourcesAsync(CancellationToken cancellationToken)
    {
        var resourceRoot = GetMlNetResourcePath();

        foreach (var (fileName, url) in GetRequiredResources(TrainerArchitecture))
        {
            var localPath = Path.Combine(resourceRoot, fileName);
            if (File.Exists(localPath))
                continue;

            _logger.LogInformation("[ModelTrainer] Đang tải resource {File} từ {Url}", fileName, url);
            await EnsureResourceFileAsync(localPath, url, cancellationToken);
            _logger.LogInformation("[ModelTrainer] Resource sẵn sàng: {Path}", localPath);
        }
    }

    private static IReadOnlyList<(string FileName, string Url)> GetRequiredResources(
        ImageClassificationTrainer.Architecture architecture)
    {
        return architecture switch
        {
            ImageClassificationTrainer.Architecture.ResnetV2101 =>
                new[]
                {
                    ("resnet_v2_101_299.meta", "https://aka.ms/mlnet-resources/meta/resnet_v2_101_299.meta")
                },
            ImageClassificationTrainer.Architecture.ResnetV250 =>
                new[]
                {
                    ("resnet_v2_50_299.meta", "https://aka.ms/mlnet-resources/meta/resnet_v2_50_299.meta")
                },
            ImageClassificationTrainer.Architecture.MobilenetV2 =>
                new[]
                {
                    ("mobilenet_v2.meta", "https://aka.ms/mlnet-resources/meta/mobilenet_v2.meta")
                },
            _ => new[]
            {
                ("inception_v3.meta", "https://aka.ms/mlnet-resources/meta/inception_v3.meta")
            }
        };
    }

    private async Task EnsureResourceFileAsync(
        string destinationPath,
        string sourceUrl,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(destinationPath);
        if (string.IsNullOrWhiteSpace(directory))
            throw new InvalidOperationException($"Đường dẫn resource không hợp lệ: {destinationPath}");

        Directory.CreateDirectory(directory);

        const int maxAttempts = 3;
        Exception? lastException = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tempFile = $"{destinationPath}.tmp";

            try
            {
                using var response = await ResourceHttpClient.GetAsync(
                    sourceUrl,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                await using (var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                await using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await responseStream.CopyToAsync(fileStream, cancellationToken);
                }

                File.Move(tempFile, destinationPath, overwrite: true);
                return;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                lastException = new TimeoutException($"Timeout khi tải resource từ {sourceUrl}.");
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }

            _logger.LogWarning(
                "[ModelTrainer] Tải resource thất bại (attempt {Attempt}/{MaxAttempts}) từ {Url}.",
                attempt,
                maxAttempts,
                sourceUrl);

            if (attempt < maxAttempts)
                await Task.Delay(TimeSpan.FromSeconds(2 * attempt), cancellationToken);
        }

        throw new InvalidOperationException(
            $"Không thể tải resource ML.NET từ '{sourceUrl}'. " +
            $"Hãy tải thủ công file '{Path.GetFileName(destinationPath)}' và đặt vào thư mục '{Path.GetDirectoryName(destinationPath)}'.",
            lastException);
    }
}
