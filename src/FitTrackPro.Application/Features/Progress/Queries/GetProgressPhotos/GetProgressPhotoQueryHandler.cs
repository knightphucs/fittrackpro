namespace FitTrackPro.Application.Features.Progress.Queries.GetProgressPhotos;

using MediatR;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Progress.DTOs;

public class GetProgressPhotosQueryHandler 
    : IRequestHandler<GetProgressPhotosQuery, Result<List<ProgressPhotoDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetProgressPhotosQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ProgressPhotoDto>>> Handle(
        GetProgressPhotosQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.ProgressPhotos
            .Where(p => p.UserId == request.UserId);

        if (!string.IsNullOrWhiteSpace(request.PhotoType))
        {
            query = query.Where(p => p.PhotoType == request.PhotoType);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(p => p.TakenAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(p => p.TakenAt <= request.EndDate.Value);
        }

        var photos = await query
            .OrderByDescending(p => p.TakenAt)
            .Select(p => new ProgressPhotoDto
            {
                Id = p.Id,
                PhotoUrl = p.PhotoUrl,
                PhotoType = p.PhotoType,
                TakenAt = p.TakenAt,
                Weight = p.Weight,
                Notes = p.Notes
            })
            .ToListAsync(cancellationToken);

        return Result<List<ProgressPhotoDto>>.Success(photos);
    }
}