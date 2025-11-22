namespace FitTrackPro.API.IntegrationTests.Services.Caching;

using FitTrackPro.Application.Common.Interfaces;

public class FakeCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        // Return default/null so the application fetches from the DB instead
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration, CancellationToken cancellationToken = default)
    {
        // Do nothing (pretend we saved it)
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        // Do nothing (pretend we deleted it)
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        // Always return false (key does not exist)
        return Task.FromResult(false);
    }
}
