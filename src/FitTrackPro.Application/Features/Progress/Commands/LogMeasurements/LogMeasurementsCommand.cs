namespace FitTrackPro.Application.Features.Progress.Commands.LogMeasurements;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Progress.DTOs;

public record LogMeasurementsCommand : IRequest<Result<ProgressEntryDto>>
{
    public Guid UserId { get; init; }
    public decimal Weight { get; init; }
    public decimal? BodyFatPercentage { get; init; }
    public decimal? Chest { get; init; }
    public decimal? Waist { get; init; }
    public decimal? Hips { get; init; }
    public decimal? Arms { get; init; }
    public decimal? Legs { get; init; }
    public DateTime? RecordedAt { get; init; }
    public string? Notes { get; init; }
}