namespace FitTrackPro.Application.Features.MealLogs.Commands.DeleteMealLog;

using MediatR;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Domain.Repositories;

public class DeleteMealLogCommandHandler : IRequestHandler<DeleteMealLogCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMealLogRepository _mealLogRepository;
    private readonly ICacheService _cacheService;

    public DeleteMealLogCommandHandler(
        IApplicationDbContext context,
        ICacheService cacheService,
        IMealLogRepository mealLogRepository)
    {
        _context = context;
        _cacheService = cacheService;
        _mealLogRepository = mealLogRepository;
    }

    public async Task<Result<Unit>> Handle(
        DeleteMealLogCommand request,
        CancellationToken cancellationToken)
    {
        // Get meal log
        var log = await _mealLogRepository.GetByIdAsync(request.MealLogId, cancellationToken);

        if (log == null || log.UserId != request.UserId)
        {
            return Result<Unit>.Failure("Meal log not found.");
        }

        // Delete meal log
        await _mealLogRepository.DeleteAsync(request.MealLogId, request.UserId, cancellationToken);

        // Invalidate cache
        var dateKey = log.LoggedAt.Date.ToString("yyyy-MM-dd");
        var cacheKey = $"meals:daily:{request.UserId}:{dateKey}";
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
