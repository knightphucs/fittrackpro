namespace FitTrackPro.Application.Features.Progress.Commands.UploadProgressPhoto;

using MediatR;
using Microsoft.AspNetCore.Http;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Progress.DTOs;

public record UploadProgressPhotoCommand : IRequest<Result<ProgressPhotoDto>>
{
    public Guid UserId { get; init; }
    public IFormFile Photo { get; init; } = default!;
    public string PhotoType { get; init; } = default!; // Front, Side, Back
    public decimal? Weight { get; init; }
    public DateTime? TakenAt { get; init; }
    public string? Notes { get; init; }
}