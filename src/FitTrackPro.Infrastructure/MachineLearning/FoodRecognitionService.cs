namespace FitTrackPro.Infrastructure.MachineLearning;
 
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Common.Utils;
using FitTrackPro.Infrastructure.MachineLearning.Models;
using FitTrackPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using Microsoft.ML.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
 
public class FoodRecognitionService : IFoodRecognitionService
{
    private readonly MLContext _mlContext;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FoodRecognitionService> _logger;
    private readonly FoodRecognitionOptions _options;
    private readonly IHostEnvironment _hostEnvironment;
 
    // Model được load 1 lần duy nhất (singleton) → thread-safe prediction engine
    private PredictionEngine<FoodImageInputData, FoodImagePrediction>? _predictionEngine;
    private VBuffer<ReadOnlyMemory<char>> _labelBuffer;
 
    public bool IsModelLoaded => _predictionEngine != null;
 
    public FoodRecognitionService(
        ApplicationDbContext context,
        ILogger<FoodRecognitionService> logger,
        IOptions<FoodRecognitionOptions> options,
        IHostEnvironment hostEnvironment)
    {
        _mlContext = new MLContext(seed: 42);
        _context = context;
        _logger = logger;
        _options = options.Value;
        _hostEnvironment = hostEnvironment;
 
        TryLoadModel();
    }
 
    // ─────────────────────────────────────────────────────
    // PUBLIC: Nhận diện món ăn từ byte[]
    // ─────────────────────────────────────────────────────
    public async Task<FoodRecognitionResult?> RecognizeAsync(
        byte[] imageBytes,
        CancellationToken cancellationToken = default)
    {
        if (!IsModelLoaded)
        {
            _logger.LogWarning("[FoodRecognition] Model chưa được load. Bỏ qua nhận diện.");
            return null;
        }
 
        try
        {
            // 1. Resize ảnh về 224x224 (chuẩn ImageNet) rồi lưu vào temp file
            var processedBytes = await PreprocessImageAsync(imageBytes);
            var tempPath = await SaveTempFileAsync(processedBytes);
 
            try
            {
                // 2. Tạo input object
                var input = new FoodImageInputData { ImagePath = tempPath };
 
                // 3. Predict
                var prediction = _predictionEngine!.Predict(input);
 
                // 4. Lấy top-3 candidates
                var topCandidates = BuildTopCandidates(prediction);
 
                // 5. Map predicted label → DB food
                var matchedFood = await FindMatchedFoodAsync(prediction.PredictedLabel, cancellationToken);
 
                var confidence = topCandidates.Count > 0 ? topCandidates[0].Confidence : 0f;
 
                _logger.LogInformation(
                    "[FoodRecognition] Predict: {Label} ({Confidence:F1}%)",
                    prediction.PredictedLabel, confidence);
 
                return new FoodRecognitionResult
                {
                    PredictedLabel    = prediction.PredictedLabel,
                    DisplayName       = FoodLabelMapper.GetDisplayName(prediction.PredictedLabel),
                    Confidence        = confidence,
                    TopCandidates     = topCandidates,
                    IsMatchedInDatabase = matchedFood != null,
                    MatchedFoodId     = matchedFood?.Id,
                    EstimatedCalories = matchedFood?.Calories,
                };
            }
            finally
            {
                // Dọn dẹp temp file
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FoodRecognition] Lỗi khi nhận diện ảnh.");
            return null;
        }
    }
 
    // ─────────────────────────────────────────────────────
    // PRIVATE HELPERS
    // ─────────────────────────────────────────────────────
 
    private void TryLoadModel()
    {
        var modelPath = ResolveModelPath(_options.ModelPath);
 
        if (!File.Exists(modelPath))
        {
            _logger.LogWarning(
                "[FoodRecognition] Model file không tồn tại tại: {Path} (configured: {ConfiguredPath}). " +
                "Chạy ModelTrainer để train model trước.",
                modelPath,
                _options.ModelPath);
            return;
        }
 
        try
        {
            var model = _mlContext.Model.Load(modelPath, out var inputSchema);
 
            // Lấy danh sách labels từ model schema để build top-k
            var labelColumn = inputSchema.GetColumnOrNull("Label");
 
            // Tạo prediction engine (thread-safe với PredictionEnginePool nếu cần scale)
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<FoodImageInputData, FoodImagePrediction>(model);
 
            _logger.LogInformation("[FoodRecognition] Model loaded thành công từ: {Path}", modelPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FoodRecognition] Không thể load model từ: {Path}", modelPath);
        }
    }

    private string ResolveModelPath(string configuredPath)
    {
        if (Path.IsPathRooted(configuredPath))
            return Path.GetFullPath(configuredPath);

        return Path.GetFullPath(Path.Combine(_hostEnvironment.ContentRootPath, configuredPath));
    }
 
    private static async Task<byte[]> PreprocessImageAsync(byte[] imageBytes)
    {
        using var image = Image.Load(imageBytes);
 
        // Resize về 224x224 (ImageNet standard)
        image.Mutate(ctx =>
        {
            ctx.Resize(new ResizeOptions
            {
                Size = new Size(224, 224),
                Mode = ResizeMode.Crop
            });
        });
 
        using var ms = new MemoryStream();
        await image.SaveAsJpegAsync(ms);
        return ms.ToArray();
    }
 
    private static async Task<string> SaveTempFileAsync(byte[] bytes)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"fittrack_food_{Guid.NewGuid()}.jpg");
        await File.WriteAllBytesAsync(tempPath, bytes);
        return tempPath;
    }
 
    private List<FoodPredictionCandidate> BuildTopCandidates(FoodImagePrediction prediction)
    {
        if (prediction.Score == null || prediction.Score.Length == 0)
            return new List<FoodPredictionCandidate>();
 
        // Lấy tất cả labels từ mapper
        var allLabels = FoodLabelMapper.All.Keys.ToArray();
 
        var candidates = prediction.Score
            .Select((score, idx) => new FoodPredictionCandidate
            {
                Label       = idx < allLabels.Length ? allLabels[idx] : $"class_{idx}",
                DisplayName = idx < allLabels.Length
                    ? FoodLabelMapper.GetDisplayName(allLabels[idx])
                    : $"Món ăn {idx}",
                Confidence  = score * 100f // 0-1 → 0-100%
            })
            .OrderByDescending(c => c.Confidence)
            .Take(3)
            .ToList();
 
        return candidates;
    }
 
    private async Task<FoodDbMatch?> FindMatchedFoodAsync(string predictedLabel, CancellationToken ct)
    {
        // Map label → tên tiếng Việt để tìm trong DB
        var displayName = FoodLabelMapper.GetDisplayName(predictedLabel);
 
        // Tìm theo NameVi HOẶC Name (case-insensitive)
        var food = await _context.Foods
            .Where(f => EF.Functions.ILike(f.NameVi, $"%{displayName}%")
                     || EF.Functions.ILike(f.Name,   $"%{predictedLabel.Replace("_", " ")}%"))
            .Select(f => new FoodDbMatch
            {
                Id                = f.Id,
                Calories = f.Calories
            })
            .FirstOrDefaultAsync(ct);
 
        return food;
    }
 
    private class FoodDbMatch
    {
        public Guid Id { get; init; }
        public int Calories { get; init; }
    }
}
 
/// <summary>
/// Internal input dùng cho PredictionEngine (khác FoodImageData dùng khi train)
/// </summary>
internal class FoodImageInputData
{
    public string ImagePath { get; set; } = default!;
}
