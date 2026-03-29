namespace FitTrackPro.Application.Common.Interfaces;

using FitTrackPro.Application.Common.Models;

public interface IModelTrainer
{
    /// <summary>
    /// Train và lưu model từ dataset ảnh trong thư mục training data.
    /// </summary>
    Task<TrainingResult> TrainAndSaveAsync(CancellationToken cancellationToken = default);
}
