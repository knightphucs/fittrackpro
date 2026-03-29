namespace FitTrackPro.Domain.Repositories;
using FitTrackPro.Domain.Entities;

public interface IMealLogRepository
{
    Task AddAsync(MealLog mealLog, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<MealLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<MealLog>> GetByUserIdAndDateRangeAsync(Guid userId, DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetRecentFoodIdsAsync(Guid userId, int count, CancellationToken cancellationToken = default);
    Task<List<MealLog>> GetRecentAsync(Guid userId, int count, CancellationToken cancellationToken = default);
    Task<List<DateTime>> GetLoggedDatesAsync(Guid userId, CancellationToken cancellationToken = default);
}