namespace FitTrackPro.Application.Features.Progress.Commands.UploadProgressPhoto;

using MediatR;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Progress.DTOs;
using FitTrackPro.Domain.Entities;
using Microsoft.AspNetCore.Http;

public class UploadProgressPhotoCommandHandler 
    : IRequestHandler<UploadProgressPhotoCommand, Result<ProgressPhotoDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private static readonly string[] AllowedTypes = new[]
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

    public UploadProgressPhotoCommandHandler(
        IApplicationDbContext context,
        IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<Result<ProgressPhotoDto>> Handle(
        UploadProgressPhotoCommand request,
        CancellationToken cancellationToken)
    {
        // Validate file
        var validationError = ValidateFile(request);
        if (validationError != null)
            return Result<ProgressPhotoDto>.Failure(validationError);

        // Upload file
        var fileName = $"progress/{request.UserId}/{Guid.NewGuid()}{Path.GetExtension(request.Photo.FileName)}";
        var photoUrl = await _fileStorage.UploadAsync(request.Photo, fileName, cancellationToken);

        // Create photo record
        var takenAt = request.TakenAt ?? DateTime.UtcNow;
        var photo = ProgressPhoto.Create(
            request.UserId,
            photoUrl,
            request.PhotoType,
            takenAt,
            request.Weight,
            request.Notes);

        _context.ProgressPhotos.Add(photo);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new ProgressPhotoDto
        {
            Id = photo.Id,
            PhotoUrl = photo.PhotoUrl,
            PhotoType = photo.PhotoType,
            TakenAt = photo.TakenAt,
            Weight = photo.Weight,
            Notes = photo.Notes
        };

        return Result<ProgressPhotoDto>.Success(dto);
    }

    private static string? ValidateFile(UploadProgressPhotoCommand request)
    {
        if (request.Photo == null || request.Photo.Length == 0)
        {
            return "Photo file is required";
        }

        if (!AllowedTypes.Contains(request.Photo.ContentType.ToLower()))
        {
            return "Invalid file type. Only JPEG, PNG, and WebP are allowed";
        }

        if (request.Photo.Length > MaxFileSizeBytes)
        {
            return "File size must not exceed 10MB";
        }

        return null;
    }
}