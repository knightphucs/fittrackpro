namespace FitTrackPro.Application.Features.Foods.Commands.TrainFoodModel;
 
using FitTrackPro.Application.Common.Models;
using MediatR;
 
/// <summary>Trigger training model từ Admin endpoint</summary>
public record TrainFoodModelCommand : IRequest<Result<TrainFoodModelDto>>;
 
public class TrainFoodModelDto
{
    public double MicroAccuracy { get; init; }
    public double MacroAccuracy { get; init; }
    public int TotalImages { get; init; }
    public int Categories { get; init; }
    public double ElapsedSeconds { get; init; }
    public bool MeetsAccuracyTarget { get; init; }
    public DateTime TrainedAt { get; init; }
    public string Message { get; init; } = default!;
}