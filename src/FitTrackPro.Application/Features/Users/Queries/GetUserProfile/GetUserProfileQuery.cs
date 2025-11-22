using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Users.DTOs;
using MediatR;

namespace FitTrackPro.Application.Features.Users.Queries.GetUserProfile;

public record GetUserProfileQuery(Guid UserId) : IRequest<Result<UserProfileDto>>;
