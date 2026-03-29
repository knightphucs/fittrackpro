namespace FitTrackPro.Application.Features.Users.Commands.UpdateProfile;

using MediatR;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Users.Commands.UpdateProfile;
using Microsoft.AspNetCore.Identity;
using FitTrackPro.Domain.Entities;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<Unit>>
{
    private readonly UserManager<User> _userManager;

    public UpdateProfileCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<Unit>> Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user == null)
            return Result<Unit>.Failure("User not found");

        user.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Gender,
            request.Height);

        var result = await _userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<Unit>.Failure($"Failed to update profile: {errors}");
        }
        return Result<Unit>.Success(Unit.Value);
    }
}
