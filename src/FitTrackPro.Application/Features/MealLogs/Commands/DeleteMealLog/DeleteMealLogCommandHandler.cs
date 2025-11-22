namespace FitTrackPro.Application.Features.MealLogs.Commands.DeleteMealLog;

using MediatR;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;

public class DeleteMealLogCommandHandler : IRequestHandler<DeleteMealLogCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public DeleteMealLogCommandHandler(
        IApplicationDbContext context,
        ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<Result<Unit>> Handle(
        DeleteMealLogCommand request,
        CancellationToken cancellationToken)
    {
        var mealLog = await _context.MealLogs
            .FirstOrDefaultAsync(m => m.Id == request.MealLogId && m.UserId == request.UserId,
                cancellationToken);

        if (mealLog == null)
        {
            return Result<Unit>.Failure("Meal log not found");
        }

        _context.MealLogs.Remove(mealLog);
        await _context.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        var dateKey = mealLog.LoggedAt.Date.ToString("yyyy-MM-dd");
        var cacheKey = $"meals:daily:{request.UserId}:{dateKey}";
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
