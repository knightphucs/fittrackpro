namespace FitTrackPro.Infrastructure.Services.Caching;

using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using FitTrackPro.Application.Common.Interfaces;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await _cache.GetStringAsync(key, cancellationToken);

        if (string.IsNullOrEmpty(value))
            return default;

        return JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
        };

        var serializedValue = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, serializedValue, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await _cache.GetStringAsync(key, cancellationToken);
        return !string.IsNullOrEmpty(value);
    }
}