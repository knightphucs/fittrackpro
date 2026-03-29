namespace FitTrackPro.Application.Features.Foods.Commands.RecognizeFood;

using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Common.Utils;
using FitTrackPro.Application.Features.Foods.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class RecognizeFoodCommandHandler
    : IRequestHandler<RecognizeFoodCommand, Result<FoodRecognitionDto>>
{
    private readonly IFoodRecognitionService _recognitionService;
    private readonly IApplicationDbContext   _context;
    private readonly ILogger<RecognizeFoodCommandHandler> _logger;

    public RecognizeFoodCommandHandler(
        IFoodRecognitionService recognitionService,
        IApplicationDbContext context,
        ILogger<RecognizeFoodCommandHandler> logger)
    {
        _recognitionService = recognitionService;
        _context            = context;
        _logger             = logger;
    }

    public async Task<Result<FoodRecognitionDto>> Handle(
        RecognizeFoodCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Kiểm tra model đã sẵn sàng chưa
        if (!_recognitionService.IsModelLoaded)
        {
            return Result<FoodRecognitionDto>.Failure(
                "Model nhận diện chưa sẵn sàng. Vui lòng liên hệ quản trị viên.");
        }

        // 2. Đọc bytes từ IFormFile
        byte[] imageBytes;
        await using (var stream = request.Image.OpenReadStream())
        {
            imageBytes = new byte[stream.Length];
            _ = await stream.ReadAsync(imageBytes, cancellationToken);
        }

        // 3. Nhận diện
        var recognitionResult = await _recognitionService.RecognizeAsync(imageBytes, cancellationToken);

        if (recognitionResult == null)
        {
            return Result<FoodRecognitionDto>.Failure(
                "Không thể nhận diện món ăn từ ảnh này. Vui lòng thử ảnh khác.");
        }

        // 4. Build top candidates với Food info từ DB
        var topCandidateDtos = await BuildCandidateDtosAsync(
            recognitionResult.TopCandidates, cancellationToken);

        // 5. Build matched food DTO nếu có
        FoodNutritionDto? matchedFoodDto = null;
        if (recognitionResult.MatchedFoodId.HasValue)
        {
            matchedFoodDto = await BuildMatchedFoodDtoAsync(
                recognitionResult.MatchedFoodId.Value, cancellationToken);
        }

        // 6. Map → DTO
        var dto = new FoodRecognitionDto
        {
            PredictedLabel = recognitionResult.PredictedLabel,
            DisplayName    = recognitionResult.DisplayName,
            Confidence     = recognitionResult.Confidence,
            TopCandidates  = topCandidateDtos,
            MatchedFood    = matchedFoodDto,
        };

        _logger.LogInformation(
            "[RecognizeFood] Label={Label}, Confidence={Confidence:F1}%, Matched={Matched}",
            dto.PredictedLabel, dto.Confidence, dto.MatchedFood != null);

        return Result<FoodRecognitionDto>.Success(dto);
    }

    // ─────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────

    private async Task<List<FoodCandidateDto>> BuildCandidateDtosAsync(
        List<FoodPredictionCandidate> candidates,
        CancellationToken ct)
    {
        var result = new List<FoodCandidateDto>();

        foreach (var candidate in candidates)
        {
            var displayName = FoodLabelMapper.GetDisplayName(candidate.Label);

            // Tìm food ID trong DB theo display name (case-insensitive)
            var labelSearch = candidate.Label.Replace("_", " ").ToLower();
            var displayNameLower = displayName.ToLower();
            var food = await _context.Foods
                .Where(f => (f.NameVi != null && f.NameVi.ToLower().Contains(displayNameLower))
                         || f.Name.ToLower().Contains(labelSearch))
                .Select(f => new { f.Id, f.Calories })
                .FirstOrDefaultAsync(ct);

            result.Add(new FoodCandidateDto
            {
                Label       = candidate.Label,
                DisplayName = displayName,
                Confidence  = candidate.Confidence,
                FoodId      = food?.Id,
                Calories    = food?.Calories,
            });
        }

        return result;
    }

    private async Task<FoodNutritionDto?> BuildMatchedFoodDtoAsync(Guid foodId, CancellationToken ct)
    {
        var food = await _context.Foods
            .Where(f => f.Id == foodId)
            .Select(f => new FoodNutritionDto
            {
                Id                 = f.Id,
                Name               = f.Name,
                NameVi             = f.NameVi ?? string.Empty,
                CaloriesPerServing = f.Calories,
                ServingSize        = f.ServingSize,
                ServingUnit        = f.ServingUnit,
                Protein            = f.Macros.Protein,
                Carbs              = f.Macros.Carbs,
                Fat                = f.Macros.Fat,
                Category           = f.Category ?? string.Empty,
            })
            .FirstOrDefaultAsync(ct);

        return food;
    }
}
