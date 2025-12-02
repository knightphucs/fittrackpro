using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Progress.DTOs;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitTrackPro.Application.Features.Progress.Commands.UpdateProgressPhoto;

public class UpdateProgressPhotoCommandHandler
    : IRequestHandler<UpdateProgressPhotoCommand, Result<ProgressPhotoDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public UpdateProgressPhotoCommandHandler(
        IApplicationDbContext context, 
        IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<Result<ProgressPhotoDto>> Handle(UpdateProgressPhotoCommand request, CancellationToken cancellationToken)
    {
        var photo = await _context.ProgressPhotos
            .FirstOrDefaultAsync(p => p.Id == request.PhotoId && p.UserId == request.UserId, cancellationToken);

        if (photo == null)
            return Result<ProgressPhotoDto>.Failure("Photo not found");

        if (request.Weight.HasValue && request.Weight.Value <= 0)
        {
            return Result<ProgressPhotoDto>.Failure("Weight must be > 0");
        }

        string? newPhotoType = null;
        if (request.PhotoType != null)
        {
            var validTypes = new HashSet<string>
            {
                PhotoTypes.Front,
                PhotoTypes.Side,
                PhotoTypes.Back,
                PhotoTypes.Custom
            };

            if (!validTypes.Contains(request.PhotoType))
            {
                return Result<ProgressPhotoDto>.Failure("Invalid photo type");
            }

            newPhotoType = request.PhotoType;
        }

        string? oldPhotoUrlToDelete = null;

        if (request.NewPhoto != null)
        {
            oldPhotoUrlToDelete = photo.PhotoUrl;

            var newFileName = $"progress/{request.UserId}/{Guid.NewGuid()}{Path.GetExtension(request.NewPhoto.FileName)}";
            var newPhotoUrl = await _fileStorage.UploadAsync(request.NewPhoto, newFileName, cancellationToken);

            photo.UpdatePhotoUrl(newPhotoUrl);
        }

        if (request.Notes != null) 
            photo.UpdateNotes(request.Notes);

        if (request.Weight.HasValue) 
            photo.UpdateWeight(request.Weight.Value);

        if (newPhotoType != null) 
            photo.ChangeType(newPhotoType);

        await _context.SaveChangesAsync(cancellationToken);

        if (oldPhotoUrlToDelete != null)
        {
            _ = _fileStorage.DeleteAsync(oldPhotoUrlToDelete, cancellationToken);
        }

        return Result<ProgressPhotoDto>.Success(new ProgressPhotoDto
        {
            Id = photo.Id,
            PhotoUrl = photo.PhotoUrl,
            PhotoType = photo.PhotoType,
            TakenAt = photo.TakenAt,
            Weight = photo.Weight,
            Notes = photo.Notes
        });
    }
}