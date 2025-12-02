namespace FitTrackPro.Application.Features.Progress.Queries.GetProgressPhotos;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Progress.DTOs;

public record GetProgressPhotosQuery(
    Guid UserId,
    string? PhotoType = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null) 
    : IRequest<Result<List<ProgressPhotoDto>>>;