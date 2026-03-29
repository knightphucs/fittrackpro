using FitTrackPro.Domain.Entities;

namespace FitTrackPro.Domain.Repositories;

public interface IWorkoutRepository
{
    Task AddAsync(WorkoutSession workout, CancellationToken cancellationToken = default);
    Task UpdateAsync(WorkoutSession workout, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkoutSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkoutSession?> GetActiveSessionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<WorkoutSession>> GetHistoryAsync(Guid userId, int count, CancellationToken cancellationToken = default);
    Task<(List<WorkoutSession> Items, long TotalCount)> GetPaginatedHistoryAsync(
        Guid userId, 
        DateTime? startDate, 
        DateTime? endDate, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default);
    Task<List<WorkoutSession>> GetCompletedWorkoutsAsync(
        Guid userId, 
        DateTime? startDate, 
        DateTime? endDate, 
        CancellationToken cancellationToken = default);
}