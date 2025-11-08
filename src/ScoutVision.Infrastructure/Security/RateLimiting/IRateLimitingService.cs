using System;
using System.Threading.Tasks;

namespace ScoutVision.Infrastructure.Security.RateLimiting;

/// <summary>
/// Rate limiting service to prevent API abuse
/// </summary>
public interface IRateLimitingService
{
    Task<RateLimitResult> CheckRateLimitAsync(string clientId, string endpoint);
    Task<RateLimitResult> CheckRateLimitAsync(string clientId, RateLimitPolicy policy);
    Task ResetRateLimitAsync(string clientId, string endpoint);
    Task<RateLimitStatus> GetRateLimitStatusAsync(string clientId, string endpoint);
    Task<bool> IsRateLimitedAsync(string clientId, string endpoint);
}

public class RateLimitResult
{
    public bool IsAllowed { get; set; }
    public int Limit { get; set; }
    public int Remaining { get; set; }
    public DateTime ResetTime { get; set; }
    public TimeSpan RetryAfter { get; set; }
    public string? Message { get; set; }
}

public class RateLimitStatus
{
    public string ClientId { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public int RequestCount { get; set; }
    public int Limit { get; set; }
    public int Remaining => Math.Max(0, Limit - RequestCount);
    public DateTime WindowStart { get; set; }
    public DateTime WindowEnd { get; set; }
    public bool IsLimited => RequestCount >= Limit;
}

public class RateLimitPolicy
{
    public string Name { get; set; } = string.Empty;
    public int RequestLimit { get; set; }
    public TimeSpan Window { get; set; }
    public RateLimitAlgorithm Algorithm { get; set; } = RateLimitAlgorithm.SlidingWindow;
    public string[]? Endpoints { get; set; }
    public string[]? ExcludedClients { get; set; }
}

public enum RateLimitAlgorithm
{
    FixedWindow,
    SlidingWindow,
    TokenBucket,
    LeakyBucket
}

/// <summary>
/// Middleware for automatic rate limiting
/// </summary>
public class RateLimitingMiddleware
{
    public static class Policies
    {
        public static RateLimitPolicy Default => new()
        {
            Name = "Default",
            RequestLimit = 100,
            Window = TimeSpan.FromMinutes(1)
        };

        public static RateLimitPolicy Strict => new()
        {
            Name = "Strict",
            RequestLimit = 10,
            Window = TimeSpan.FromMinutes(1)
        };

        public static RateLimitPolicy Generous => new()
        {
            Name = "Generous",
            RequestLimit = 1000,
            Window = TimeSpan.FromMinutes(1)
        };

        public static RateLimitPolicy VideoAnalysis => new()
        {
            Name = "VideoAnalysis",
            RequestLimit = 5,
            Window = TimeSpan.FromHours(1)
        };

        public static RateLimitPolicy DataExport => new()
        {
            Name = "DataExport",
            RequestLimit = 20,
            Window = TimeSpan.FromHours(1)
        };
    }
}

