namespace FitTrackPro.Application.Features.Users.Commands.UpdateProfile;

using MediatR;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public UpdateProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FindAsync(new object[] { request.UserId }, cancellationToken);

        if (user == null)
            return Result<Unit>.Failure("User not found");

        user.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Gender,
            request.Height);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}