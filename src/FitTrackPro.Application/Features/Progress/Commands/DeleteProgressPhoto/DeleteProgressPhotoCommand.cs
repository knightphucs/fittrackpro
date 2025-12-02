using FitTrackPro.Application.Common.Models;
using MediatR;

namespace FitTrackPro.Application.Features.Progress.Commands.DeleteProgressPhoto;

public record DeleteProgressPhotoCommand(Guid PhotoId, Guid UserId) : IRequest<Result<Unit>>;