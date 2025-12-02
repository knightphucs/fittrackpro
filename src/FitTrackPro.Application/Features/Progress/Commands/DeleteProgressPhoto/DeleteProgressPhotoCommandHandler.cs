using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitTrackPro.Application.Features.Progress.Commands.DeleteProgressPhoto;

public class DeleteProgressPhotoCommandHandler : IRequestHandler<DeleteProgressPhotoCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public DeleteProgressPhotoCommandHandler(
        IApplicationDbContext context,
        IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<Result<Unit>> Handle(
        DeleteProgressPhotoCommand request,
        CancellationToken cancellationToken)
    {
        var photo = await _context.ProgressPhotos
            .FirstOrDefaultAsync(p => p.Id == request.PhotoId && p.UserId == request.UserId, cancellationToken);

        if (photo == null)
        {
            return Result<Unit>.Failure("Photo not found");
        }

        // Delete file from storage
        await _fileStorage.DeleteAsync(photo.PhotoUrl, cancellationToken);

        // Remove record from database
        _context.ProgressPhotos.Remove(photo);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}