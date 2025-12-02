using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Progress.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace FitTrackPro.Application.Features.Progress.Commands.UpdateProgressPhoto;

public class UpdateProgressPhotoCommand : IRequest<Result<ProgressPhotoDto>>
{
    public Guid PhotoId { get; set; }

    // Bound from Form
    public IFormFile? NewPhoto { get; set; }
    public string? Notes { get; set; }
    public decimal? Weight { get; set; }
    public string? PhotoType { get; set; }

    [JsonIgnore]
    public Guid UserId { get; set; }
}
