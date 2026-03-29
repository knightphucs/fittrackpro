namespace FitTrackPro.Domain.Repositories;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Enums;

public interface IPersonalRecordRepository
{
    Task AddAsync(PersonalRecord pr, CancellationToken cancellationToken = default);
    Task<PersonalRecord?> GetBestByTypeAsync(Guid userId, Guid exerciseId, PersonalRecordType type, CancellationToken cancellationToken = default);
    Task<List<PersonalRecord>> GetByUserIdAsync(Guid userId, Guid? exerciseId = null, CancellationToken cancellationToken = default);
}