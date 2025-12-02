namespace FitTrackPro.Application.Features.Progress.Commands.UploadMultiplePhotos;

using MediatR;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Progress.DTOs;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Enums;

public class UploadMultiplePhotosCommandHandler 
    : IRequestHandler<UploadMultiplePhotosCommand, Result<List<ProgressPhotoDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public UploadMultiplePhotosCommandHandler(
        IApplicationDbContext context,
        IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<Result<List<ProgressPhotoDto>>> Handle(
        UploadMultiplePhotosCommand request,
        CancellationToken cancellationToken)
    {

        if (request.Photos.Count != request.PhotoTypes.Count)
        {
            return Result<List<ProgressPhotoDto>>.Failure(
                "Number of photos must match number of photo types");
        }

        var takenAt = request.TakenAt ?? DateTime.UtcNow;
        var photoDtos = new List<ProgressPhotoDto>();

        var allowedTypes = new List<string>
        {
            PhotoTypes.Front,
            PhotoTypes.Side,
            PhotoTypes.Back,
            PhotoTypes.Custom
        };

        for (int i = 0; i < request.Photos.Count; i++)
        {
            var photo = request.Photos[i];
            var photoType = request.PhotoTypes[i];

            // Validate
            if (photo == null || photo.Length == 0)
            {
                continue;
            }

            // Validate type
            if (!allowedTypes.Contains(photoType))
            {
                return Result<List<ProgressPhotoDto>>.Failure(
                    $"Invalid photo type: {photoType}. Allowed: Front, Side, Back, Custom");
            }

            // Upload with compression
            var fileName = $"progress/{request.UserId}/{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
            var photoUrl = await _fileStorage.UploadWithCompressionAsync(
                photo, fileName, cancellationToken: cancellationToken);

            // Create record
            var progressPhoto = ProgressPhoto.Create(
                request.UserId,
                photoUrl,
                photoType,
                takenAt,
                request.Weight,
                request.Notes);

            _context.ProgressPhotos.Add(progressPhoto);

            photoDtos.Add(new ProgressPhotoDto
            {
                Id = progressPhoto.Id,
                PhotoUrl = progressPhoto.PhotoUrl,
                PhotoType = progressPhoto.PhotoType,
                TakenAt = progressPhoto.TakenAt,
                Weight = progressPhoto.Weight,
                Notes = progressPhoto.Notes
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<List<ProgressPhotoDto>>.Success(photoDtos);
    }
}