using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Users.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitTrackPro.Application.Features.Users.Queries.GetUserProfile;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    private readonly IApplicationDbContext _context;
    public GetUserProfileQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.CurrentGoal)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            return Result<UserProfileDto>.Failure("User not found");

        var dto = new UserProfileDto
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.GetFullName(),
            DateOfBirth = user.DateOfBirth,
            Age = user.GetAge(),
            Gender = user.Gender,
            Height = user.Height,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            IsEmailConfirmed = user.EmailConfirmed,
            HasActiveGoal = user.CurrentGoal?.IsActive ?? false
        };

        return Result<UserProfileDto>.Success(dto);
    }
}
