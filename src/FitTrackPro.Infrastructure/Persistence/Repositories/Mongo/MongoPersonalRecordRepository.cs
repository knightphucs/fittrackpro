namespace FitTrackPro.Infrastructure.Persistence.Repositories.Mongo;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Repositories;
using FitTrackPro.Domain.Enums;
using MongoDB.Driver;
using System.Collections.Generic;

public class MongoPersonalRecordRepository : IPersonalRecordRepository
{
    private readonly IMongoCollection<PersonalRecord> _collection;

    public MongoPersonalRecordRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<PersonalRecord>("personal_records");
    }

    public async Task AddAsync(PersonalRecord pr, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(pr, null, cancellationToken);
    }

    public async Task<PersonalRecord?> GetBestByTypeAsync(Guid userId, Guid exerciseId, PersonalRecordType type, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(x => x.UserId == userId && x.ExerciseId == exerciseId && x.Type == type)
            .SortByDescending(x => x.Value)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<PersonalRecord>> GetByUserIdAsync(Guid userId, Guid? exerciseId = null, CancellationToken cancellationToken = default)
    {
        var builder = Builders<PersonalRecord>.Filter;
        var filter = builder.Eq(pr => pr.UserId, userId);

        if (exerciseId.HasValue)
        {
            filter &= builder.Eq(pr => pr.ExerciseId, exerciseId.Value);
        }

        return await _collection.Find(filter)
            .SortByDescending(pr => pr.AchievedAt)
            .ToListAsync(cancellationToken);
    }
}