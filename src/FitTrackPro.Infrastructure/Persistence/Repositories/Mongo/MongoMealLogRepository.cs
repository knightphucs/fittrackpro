namespace FitTrackPro.Infrastructure.Persistence.Repositories.Mongo;

using System.Collections.Generic;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Repositories;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

public class MongoMealLogRepository : IMealLogRepository
{
    private readonly IMongoCollection<MealLog> _collection;

    public MongoMealLogRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<MealLog>("meal_logs");

        var indexKeys = Builders<MealLog>.IndexKeys;
        var indexModel = new CreateIndexModel<MealLog>(
            indexKeys.Ascending(x => x.UserId).Descending(x => x.LoggedAt)
        );
        _collection.Indexes.CreateOne(indexModel);
    }

    public async Task AddAsync(MealLog mealLog, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(mealLog, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<MealLog>.Filter.And(
            Builders<MealLog>.Filter.Eq(x => x.Id, id),
            Builders<MealLog>.Filter.Eq(x => x.UserId, userId)
        );
        
        await _collection.DeleteOneAsync(filter, cancellationToken);
    }

    public async Task<MealLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<MealLog>.Filter.Eq(x => x.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<MealLog>> GetByUserIdAndDateRangeAsync(Guid userId, DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<MealLog>.Filter;
        
        var filter = filterBuilder.Eq(x => x.UserId, userId) &
                     filterBuilder.Gte(x => x.LoggedAt, start) &
                     filterBuilder.Lte(x => x.LoggedAt, end);

        var sort = Builders<MealLog>.Sort.Ascending(x => x.LoggedAt);

        return await _collection.Find(filter).Sort(sort).ToListAsync(cancellationToken);
    }

    public async Task<List<DateTime>> GetLoggedDatesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var query = _collection.AsQueryable()
            .Where(x => x.UserId == userId)
            .Select(x => x.LoggedAt)
            .Distinct();

        var dates = await query.ToListAsync(cancellationToken);

        return dates.Select(d => d.Date).Distinct().ToList();
    }

    public async Task<List<MealLog>> GetRecentAsync(Guid userId, int count, CancellationToken cancellationToken = default)
    {
        var filter = Builders<MealLog>.Filter.Eq(x => x.UserId, userId);
        var sort = Builders<MealLog>.Sort.Descending(x => x.LoggedAt);
        
        return await _collection.Find(filter).Sort(sort).Limit(count).ToListAsync(cancellationToken);
    }

    public async Task<List<Guid>> GetRecentFoodIdsAsync(Guid userId, int count, CancellationToken cancellationToken = default)
    {
            var query = _collection.AsQueryable()
            .Where(x => x.UserId == userId)
            .GroupBy(x => x.FoodSnapshot.OriginalFoodId)
            .Select(g => new 
            { 
                FoodId = g.Key, 
                LastEaten = g.Max(x => x.LoggedAt)
            })
            .OrderByDescending(x => x.LastEaten)
            .Take(count) // Limit số lượng
            .Select(x => x.FoodId);

        return await query.ToListAsync(cancellationToken);
    }
}