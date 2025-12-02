namespace FitTrackPro.Application.Features.Progress.Commands.UploadMultiplePhotos;

using MediatR;
using Microsoft.AspNetCore.Http;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Progress.DTOs;

public record UploadMultiplePhotosCommand : IRequest<Result<List<ProgressPhotoDto>>>
{
    public Guid UserId { get; init; }
    public List<IFormFile> Photos { get; init; } = new();
    public List<string> PhotoTypes { get; init; } = new(); // Front, Side, Back
    public decimal? Weight { get; init; }
    public DateTime? TakenAt { get; init; }
    public string? Notes { get; init; }
}