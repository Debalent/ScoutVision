namespace ScoutVision.Infrastructure.Caching;

/// <summary>
/// Advanced caching service with multi-level caching support
/// </summary>
public interface ICacheService
{
    // Basic operations
    Task<T> GetAsync<T>(string key);
    Task<string> GetAsync(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task SetAsync(string key, string value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
    Task<bool> ExistsAsync(string key);
    Task<long> IncrementAsync(string key);
    Task<long> DecrementAsync(string key);

    // Advanced operations
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
    Task<Dictionary<string, T>> GetManyAsync<T>(IEnumerable<string> keys);
    Task SetManyAsync<T>(Dictionary<string, T> items, TimeSpan? expiration = null);
    Task RemoveManyAsync(IEnumerable<string> keys);

    // Cache invalidation
    Task InvalidateCacheGroupAsync(string groupName);
    Task RefreshAsync(string key);
    Task<long> GetTTLAsync(string key);

    // Cache statistics
    Task<CacheStatistics> GetStatisticsAsync();
    Task<long> GetCacheSizeAsync();
    Task ClearAllAsync();
}

public class CacheStatistics
{
    public long TotalKeys { get; set; }
    public long HitCount { get; set; }
    public long MissCount { get; set; }
    public decimal HitRate => TotalRequests > 0 ? (decimal)HitCount / TotalRequests * 100 : 0;
    public long TotalRequests => HitCount + MissCount;
    public long MemoryUsageBytes { get; set; }
    public Dictionary<string, long> KeysByPrefix { get; set; } = new();
}