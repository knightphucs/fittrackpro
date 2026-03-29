namespace FitTrackPro.Infrastructure.Persistence.Repositories.Mongo;

using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Repositories;
using FitTrackPro.Domain.Enums;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

public class MongoWorkoutRepository : IWorkoutRepository
{
    private readonly IMongoCollection<WorkoutSession> _collection;

    public MongoWorkoutRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<WorkoutSession>("workout_sessions");
    }

    public async Task AddAsync(WorkoutSession workout, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(workout, null, cancellationToken);
    }

    public async Task UpdateAsync(WorkoutSession workout, CancellationToken cancellationToken = default)
    {
        await _collection.ReplaceOneAsync(
            w => w.Id == workout.Id, 
            workout, 
            new ReplaceOptions(),
            cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _collection.DeleteOneAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<WorkoutSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(w => w.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<WorkoutSession?> GetActiveSessionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(w => w.UserId == userId && w.Status == WorkoutStatus.InProgress)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<WorkoutSession>> GetHistoryAsync(Guid userId, int count, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(w => w.UserId == userId)
            .SortByDescending(w => w.StartedAt)
            .Limit(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<WorkoutSession> Items, long TotalCount)> GetPaginatedHistoryAsync(Guid userId, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var builder = Builders<WorkoutSession>.Filter;
        var filter = builder.Eq(w => w.UserId, userId);

        if (startDate.HasValue)
        {
            filter &= builder.Gte(w => w.StartedAt, startDate.Value);
        }

        if (endDate.HasValue)
        {
            filter &= builder.Lte(w => w.StartedAt, endDate.Value);
        }

        var totalCount = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var items = await _collection
            .Find(filter)
            .SortByDescending(w => w.StartedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<WorkoutSession>> GetCompletedWorkoutsAsync(Guid userId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        var builder = Builders<WorkoutSession>.Filter;
        var filter = builder.Eq(w => w.UserId, userId) &
                     builder.Eq(w => w.Status, WorkoutStatus.Completed);

        if (startDate.HasValue)
        {
            filter &= builder.Gte(w => w.StartedAt, startDate.Value);
        }

        if (endDate.HasValue)
        {
            filter &= builder.Lte(w => w.StartedAt, endDate.Value);
        }

        return await _collection
            .Find(filter)
            .SortByDescending(w => w.StartedAt)
            .ToListAsync(cancellationToken);
    }
}