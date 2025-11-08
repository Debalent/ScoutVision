namespace ScoutVision.Infrastructure.Caching;

public interface ICacheService
{
    Task<T> GetAsync<T>(string key);
    Task<string> GetAsync(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task SetAsync(string key, string value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
    Task<bool> ExistsAsync(string key);
    Task<long> IncrementAsync(string key);
    Task<long> DecrementAsync(string key);
}