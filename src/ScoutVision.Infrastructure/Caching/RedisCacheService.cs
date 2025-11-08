using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ScoutVision.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _connection;
    private readonly ILogger<RedisCacheService> _logger;
    private IDatabase Database => _connection.GetDatabase();

    public RedisCacheService(IConnectionMultiplexer connection, ILogger<RedisCacheService> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<T> GetAsync<T>(string key)
    {
        try
        {
            var value = await Database.StringGetAsync(key);
            if (value.IsNull)
                return default;

            return JsonSerializer.Deserialize<T>(value.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting cache key: {key}");
            return default;
        }
    }

    public async Task<string> GetAsync(string key)
    {
        try
        {
            var value = await Database.StringGetAsync(key);
            return value.IsNull ? null : value.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting cache key: {key}");
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await Database.StringSetAsync(key, json, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting cache key: {key}");
        }
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiration = null)
    {
        try
        {
            await Database.StringSetAsync(key, value, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting cache key: {key}");
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await Database.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing cache key: {key}");
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var server = _connection.GetServer(_connection.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern);
            
            foreach (var key in keys)
            {
                await Database.KeyDeleteAsync(key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing cache pattern: {pattern}");
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return await Database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking cache key: {key}");
            return false;
        }
    }

    public async Task<long> IncrementAsync(string key)
    {
        try
        {
            return await Database.StringIncrementAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error incrementing cache key: {key}");
            return 0;
        }
    }

    public async Task<long> DecrementAsync(string key)
    {
        try
        {
            return await Database.StringDecrementAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error decrementing cache key: {key}");
            return 0;
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        try
        {
            var cached = await GetAsync<T>(key);
            if (cached != null)
                return cached;

            var value = await factory();
            await SetAsync(key, value, expiration);
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in GetOrSet for key: {key}");
            return default;
        }
    }

    public async Task<Dictionary<string, T>> GetManyAsync<T>(IEnumerable<string> keys)
    {
        var result = new Dictionary<string, T>();
        try
        {
            foreach (var key in keys)
            {
                var value = await GetAsync<T>(key);
                if (value != null)
                    result[key] = value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting multiple cache keys");
        }
        return result;
    }

    public async Task SetManyAsync<T>(Dictionary<string, T> items, TimeSpan? expiration = null)
    {
        try
        {
            foreach (var item in items)
            {
                await SetAsync(item.Key, item.Value, expiration);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting multiple cache keys");
        }
    }

    public async Task RemoveManyAsync(IEnumerable<string> keys)
    {
        try
        {
            foreach (var key in keys)
            {
                await RemoveAsync(key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing multiple cache keys");
        }
    }

    public async Task InvalidateCacheGroupAsync(string groupName)
    {
        await RemoveByPatternAsync($"{groupName}:*");
    }

    public async Task RefreshAsync(string key)
    {
        try
        {
            var ttl = await Database.KeyTimeToLiveAsync(key);
            if (ttl.HasValue)
            {
                await Database.KeyExpireAsync(key, ttl.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error refreshing cache key: {key}");
        }
    }

    public async Task<long> GetTTLAsync(string key)
    {
        try
        {
            var ttl = await Database.KeyTimeToLiveAsync(key);
            return ttl.HasValue ? (long)ttl.Value.TotalSeconds : -1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting TTL for key: {key}");
            return -1;
        }
    }

    public async Task<CacheStatistics> GetStatisticsAsync()
    {
        try
        {
            var server = _connection.GetServer(_connection.GetEndPoints().First());
            var info = await server.InfoAsync("stats");
            var memory = await server.InfoAsync("memory");

            var memoryGroup = memory.FirstOrDefault();
            var usedMemoryEntry = memoryGroup?.FirstOrDefault(x => x.Key == "used_memory");

            var stats = new CacheStatistics
            {
                TotalKeys = (long)await Database.ExecuteAsync("DBSIZE"),
                MemoryUsageBytes = long.Parse(usedMemoryEntry?.Value ?? "0")
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache statistics");
            return new CacheStatistics();
        }
    }

    public async Task<long> GetCacheSizeAsync()
    {
        try
        {
            return (long)await Database.ExecuteAsync("DBSIZE");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache size");
            return 0;
        }
    }

    public async Task ClearAllAsync()
    {
        try
        {
            var server = _connection.GetServer(_connection.GetEndPoints().First());
            await server.FlushDatabaseAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all cache");
        }
    }
}