namespace FitTrackPro.Infrastructure.MachineLearning;
 
public class FoodRecognitionOptions
{
    public const string SectionName = "FoodRecognition";
 
    /// <summary>Đường dẫn tới model .zip (relative hoặc absolute)</summary>
    public string ModelPath { get; set; } = "ml-models/food_recognition_model.zip";
 
    /// <summary>Đường dẫn thư mục training data</summary>
    public string TrainingDataPath { get; set; } = "scripts/training-data";
 
    /// <summary>Số epoch khi train</summary>
    public int Epochs { get; set; } = 100;
 
    /// <summary>Batch size</summary>
    public int BatchSize { get; set; } = 10;
 
    /// <summary>Ngưỡng confidence tối thiểu (%) để coi là "đáng tin"</summary>
    public float ConfidenceThreshold { get; set; } = 70f;
}