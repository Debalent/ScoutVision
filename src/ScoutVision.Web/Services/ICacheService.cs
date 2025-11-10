using System;
using System.Threading.Tasks;

namespace ScoutVision.Web.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task ClearAsync();
}

public class MemoryCacheService : ICacheService
{
    private readonly Dictionary<string, CacheItem> _cache = new();
    private readonly object _lock = new();

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(key, out var item))
            {
                if (item.ExpiresAt == null || item.ExpiresAt > DateTime.UtcNow)
                {
                    return Task.FromResult(item.Value as T);
                }
                else
                {
                    _cache.Remove(key);
                }
            }
            return Task.FromResult<T?>(null);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        lock (_lock)
        {
            var expiresAt = expiration.HasValue ? (DateTime?)DateTime.UtcNow.Add(expiration.Value) : null;
            _cache[key] = new CacheItem { Value = value, ExpiresAt = expiresAt };
        }
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        lock (_lock)
        {
            _cache.Remove(key);
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(key, out var item))
            {
                if (item.ExpiresAt == null || item.ExpiresAt > DateTime.UtcNow)
                {
                    return Task.FromResult(true);
                }
                else
                {
                    _cache.Remove(key);
                }
            }
            return Task.FromResult(false);
        }
    }

    public Task ClearAsync()
    {
        lock (_lock)
        {
            _cache.Clear();
        }
        return Task.CompletedTask;
    }

    private class CacheItem
    {
        public object? Value { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}