using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace Wheelzy.Infrastructure.Caching;

/// <summary>
/// Reference cache usage for data that is read constantly and changes rarely
/// (makes, models, zip coverage, status types, buyer standard quotes).
/// </summary>
public sealed class LookupCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache? _distributedCache;

    public LookupCache(IMemoryCache memoryCache, IDistributedCache? distributedCache = null)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan timeToLive,
        CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(key, out T? local) && local is not null)
        {
            return local;
        }

        if (_distributedCache is not null)
        {
            var payload = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (!string.IsNullOrEmpty(payload))
            {
                var deserialized = System.Text.Json.JsonSerializer.Deserialize<T>(payload);
                if (deserialized is not null)
                {
                    _memoryCache.Set(key, deserialized, timeToLive);
                    return deserialized;
                }
            }
        }

        var value = await factory(cancellationToken);
        _memoryCache.Set(key, value, timeToLive);

        if (_distributedCache is not null)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(
                key,
                json,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = timeToLive },
                cancellationToken);
        }

        return value;
    }

    /// <summary>
    /// Call after the underlying data changes (for example an admin edits the makes list).
    /// Removes the local copy and the shared Redis copy so other instances reload it.
    /// </summary>
    public async Task InvalidateAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);

        if (_distributedCache is not null)
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
        }
    }
}
