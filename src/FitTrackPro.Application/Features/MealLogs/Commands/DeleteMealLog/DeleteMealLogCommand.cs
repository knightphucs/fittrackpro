namespace FitTrackPro.Application.Features.MealLogs.Commands.DeleteMealLog;

using MediatR;
using FitTrackPro.Application.Common.Models;

public record DeleteMealLogCommand(Guid UserId, Guid MealLogId) : IRequest<Result<Unit>>;
