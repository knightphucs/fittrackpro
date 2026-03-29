namespace FitTrackPro.Application.Common.Interfaces;

using FitTrackPro.Application.Common.Models;

public interface IFoodRecognitionService
{
    /// <summary>
    /// Nhận diện món ăn từ byte array ảnh upload
    /// </summary>
    Task<FoodRecognitionResult?> RecognizeAsync(byte[] imageBytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra model đã được load chưa
    /// </summary>
    bool IsModelLoaded { get; }
}
