namespace FitTrackPro.Application.Features.Progress.Commands.LogWeight;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Progress.DTOs;

public record LogWeightCommand : IRequest<Result<ProgressEntryDto>>
{
    public Guid UserId { get; init; }
    public decimal Weight { get; init; }
    public DateTime? RecordedAt { get; init; }
    public string? Notes { get; init; }
}