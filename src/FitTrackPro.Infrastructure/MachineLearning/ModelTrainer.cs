namespace FitTrackPro.Infrastructure.MachineLearning;

using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Infrastructure.MachineLearning.Models;
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
    private readonly MLContext _mlContext;
    private readonly FoodRecognitionOptions _options;
    private readonly ILogger<ModelTrainer> _logger;

    public ModelTrainer(
        IOptions<FoodRecognitionOptions> options,
        ILogger<ModelTrainer> logger)
    {
        _mlContext = new MLContext(seed: 42);
        _options = options.Value;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────
    // TRAIN & SAVE MODEL
    // ─────────────────────────────────────────────────────
    public async Task<TrainingResult> TrainAndSaveAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[ModelTrainer] Bắt đầu quá trình training...");

        // 1. Kiểm tra thư mục data
        if (!Directory.Exists(_options.TrainingDataPath))
            throw new DirectoryNotFoundException(
                $"Không tìm thấy thư mục training data: {_options.TrainingDataPath}");

        // 2. Load data từ các subfolders (mỗi folder = 1 label)
        var imageData = LoadImagesFromFolders(_options.TrainingDataPath);
        _logger.LogInformation("[ModelTrainer] Tổng ảnh: {Count}", imageData.Count);

        if (imageData.Count < 20)
            throw new InvalidOperationException(
                "Cần ít nhất 20 ảnh để train. Thêm ảnh vào thư mục training-data/.");

        // 3. Tạo IDataView
        var dataView = _mlContext.Data.LoadFromEnumerable(imageData);

        // 4. Shuffle và split 80/20
        var shuffled = _mlContext.Data.ShuffleRows(dataView);
        var split    = _mlContext.Data.TrainTestSplit(shuffled, testFraction: 0.2);

        // 5. Build training pipeline (Transfer Learning với ResNet)
        var pipeline = BuildPipeline();

        // 6. Train (async để không block thread pool)
        _logger.LogInformation("[ModelTrainer] Đang train... (có thể mất vài phút)");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var model = await Task.Run(() => pipeline.Fit(split.TrainSet), cancellationToken);

        stopwatch.Stop();
        _logger.LogInformation("[ModelTrainer] Train xong sau {Seconds}s", stopwatch.Elapsed.TotalSeconds);

        // 7. Evaluate trên test set
        var predictions  = model.Transform(split.TestSet);
        var metrics      = _mlContext.MulticlassClassification.Evaluate(predictions, "LabelKey", "PredictedLabel");

        var result = new TrainingResult
        {
            MicroAccuracy = metrics.MicroAccuracy,
            MacroAccuracy = metrics.MacroAccuracy,
            LogLoss       = metrics.LogLoss,
            TrainedAt     = DateTime.UtcNow,
            TotalImages   = imageData.Count,
            Categories    = imageData.Select(i => i.Label).Distinct().Count(),
            ElapsedSeconds = stopwatch.Elapsed.TotalSeconds,
        };

        _logger.LogInformation(
            "[ModelTrainer] Kết quả: MicroAccuracy={MicroAcc:F3}, MacroAccuracy={MacroAcc:F3}",
            result.MicroAccuracy, result.MacroAccuracy);

        // 8. Save model
        EnsureModelDirectory();
        _mlContext.Model.Save(model, dataView.Schema, _options.ModelPath);
        _logger.LogInformation("[ModelTrainer] Model đã lưu tại: {Path}", _options.ModelPath);

        return result;
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
                Arch                 = ImageClassificationTrainer.Architecture.ResnetV2101,
                Epoch                = _options.Epochs,
                BatchSize            = _options.BatchSize,
                LearningRate         = 0.01f,
                ValidationSet        = null, // dùng split ở trên
                ReuseTrainSetBottleneckCachedValues = true,
                MetricsCallback      = (metrics) =>
                {
                    if (metrics.Train.BatchProcessedCount % 10 == 0)
                        _logger.LogDebug("[ModelTrainer] Batch {Batch}: Accuracy={Acc:F3}",
                            metrics.Train.BatchProcessedCount, metrics.Train.Accuracy);
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

    private void EnsureModelDirectory()
    {
        var dir = Path.GetDirectoryName(_options.ModelPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }
}
