using StackExchange.Redis;
using System.Text.Json;

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
}