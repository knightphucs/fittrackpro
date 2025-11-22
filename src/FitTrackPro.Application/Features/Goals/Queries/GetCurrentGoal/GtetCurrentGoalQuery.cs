using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Goals.DTOs;
using MediatR;

namespace FitTrackPro.Application.Features.Goals.Queries.GetCurrentGoal;

public record GetCurrentGoalQuery(Guid UserId) : IRequest<Result<UserGoalDto>>;
