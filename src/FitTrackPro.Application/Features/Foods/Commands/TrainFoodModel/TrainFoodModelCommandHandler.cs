namespace FitTrackPro.Application.Features.Foods.Commands.TrainFoodModel;

using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

public class TrainFoodModelCommandHandler
    : IRequestHandler<TrainFoodModelCommand, Result<TrainFoodModelDto>>
{
    private readonly IModelTrainer _trainer;
    private readonly ILogger<TrainFoodModelCommandHandler> _logger;

    public TrainFoodModelCommandHandler(
        IModelTrainer trainer,
        ILogger<TrainFoodModelCommandHandler> logger)
    {
        _trainer = trainer;
        _logger  = logger;
    }

    public async Task<Result<TrainFoodModelDto>> Handle(
        TrainFoodModelCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("[TrainFoodModel] Admin triggered model training.");

            var result = await _trainer.TrainAndSaveAsync(cancellationToken);

            var dto = new TrainFoodModelDto
            {
                MicroAccuracy      = result.MicroAccuracy,
                MacroAccuracy      = result.MacroAccuracy,
                TotalImages        = result.TotalImages,
                Categories         = result.Categories,
                ElapsedSeconds     = result.ElapsedSeconds,
                MeetsAccuracyTarget = result.MeetsAccuracyTarget,
                TrainedAt          = result.TrainedAt,
                Message = result.MeetsAccuracyTarget
                    ? $"Training thành công! Accuracy đạt {result.MicroAccuracy:P1} (>= 85%)."
                    : $"Training hoàn tất nhưng accuracy {result.MicroAccuracy:P1} < 85%. Cần thêm ảnh training.",
            };

            return Result<TrainFoodModelDto>.Success(dto);
        }
        catch (DirectoryNotFoundException ex)
        {
            return Result<TrainFoodModelDto>.Failure(
                $"Không tìm thấy thư mục training data: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return Result<TrainFoodModelDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TrainFoodModel] Lỗi không mong muốn khi training.");
            return Result<TrainFoodModelDto>.Failure("Lỗi hệ thống khi train model.");
        }
    }
}
