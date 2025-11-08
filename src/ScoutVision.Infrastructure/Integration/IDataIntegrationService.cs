using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScoutVision.Infrastructure.Integration;

/// <summary>
/// Service for integrating with third-party sports data providers
/// </summary>
public interface IDataIntegrationService
{
    // Provider management
    Task<List<DataProvider>> GetAvailableProvidersAsync();
    Task<DataProvider> GetProviderAsync(string providerId);
    Task<bool> EnableProviderAsync(string providerId, Dictionary<string, string> credentials);
    Task<bool> DisableProviderAsync(string providerId);
    Task<ProviderStatus> GetProviderStatusAsync(string providerId);
    
    // Data synchronization
    Task<SyncResult> SyncPlayerDataAsync(string providerId, int playerId);
    Task<SyncResult> SyncTeamDataAsync(string providerId, int teamId);
    Task<SyncResult> SyncMatchDataAsync(string providerId, int matchId);
    Task<SyncResult> SyncLeagueDataAsync(string providerId, string leagueId);
    Task<SyncResult> SyncAllDataAsync(string providerId);
    
    // Scheduled sync
    Task<string> ScheduleSyncAsync(SyncSchedule schedule);
    Task<List<SyncSchedule>> GetSyncSchedulesAsync();
    Task<bool> UpdateSyncScheduleAsync(SyncSchedule schedule);
    Task<bool> CancelSyncScheduleAsync(string scheduleId);
    
    // Sync history
    Task<List<SyncHistory>> GetSyncHistoryAsync(string providerId, int page = 1, int pageSize = 20);
    Task<SyncStatistics> GetSyncStatisticsAsync(string providerId);
}

public class DataProvider
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public DataProviderType Type { get; set; }
    public List<DataCategory> SupportedCategories { get; set; } = new();
    public List<string> SupportedLeagues { get; set; } = new();
    public PricingTier PricingTier { get; set; }
    public bool RequiresApiKey { get; set; }
    public bool IsEnabled { get; set; }
    public Dictionary<string, string>? ConfigurationFields { get; set; }
}

public enum DataProviderType
{
    Opta,
    StatsBomb,
    Wyscout,
    TransferMarkt,
    SofaScore,
    Understat,
    FBRef,
    InStat,
    Custom
}

public enum DataCategory
{
    PlayerStatistics,
    TeamStatistics,
    MatchEvents,
    TransferData,
    MarketValues,
    InjuryData,
    TacticalData,
    VideoFootage,
    TrackingData,
    PhysicalData
}

public enum PricingTier
{
    Free,
    Basic,
    Professional,
    Enterprise
}

public class ProviderStatus
{
    public string ProviderId { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    public bool IsHealthy { get; set; }
    public DateTime? LastSync { get; set; }
    public DateTime? NextScheduledSync { get; set; }
    public int ApiCallsToday { get; set; }
    public int ApiCallLimit { get; set; }
    public string? ErrorMessage { get; set; }
}

public class SyncResult
{
    public bool Success { get; set; }
    public string ProviderId { get; set; } = string.Empty;
    public int RecordsProcessed { get; set; }
    public int RecordsCreated { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsFailed { get; set; }
    public TimeSpan Duration { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
}

public class SyncSchedule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProviderId { get; set; } = string.Empty;
    public DataCategory Category { get; set; }
    public SyncFrequency Frequency { get; set; }
    public TimeSpan Time { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastRun { get; set; }
    public DateTime? NextRun { get; set; }
    public Dictionary<string, object>? Filters { get; set; }
}

public enum SyncFrequency
{
    Hourly,
    Daily,
    Weekly,
    Monthly,
    RealTime
}

public class SyncHistory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProviderId { get; set; } = string.Empty;
    public DataCategory Category { get; set; }
    public SyncResult Result { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public string? TriggeredBy { get; set; }
}

public class SyncStatistics
{
    public string ProviderId { get; set; } = string.Empty;
    public int TotalSyncs { get; set; }
    public int SuccessfulSyncs { get; set; }
    public int FailedSyncs { get; set; }
    public decimal SuccessRate => TotalSyncs > 0 ? (decimal)SuccessfulSyncs / TotalSyncs * 100 : 0;
    public int TotalRecordsProcessed { get; set; }
    public DateTime? LastSuccessfulSync { get; set; }
    public TimeSpan AverageSyncDuration { get; set; }
}

/// <summary>
/// Opta Sports integration service
/// </summary>
public interface IOptaIntegrationService
{
    Task<OptaMatchData> GetMatchDataAsync(string matchId);
    Task<OptaPlayerStats> GetPlayerStatsAsync(int playerId, string season);
    Task<List<OptaEvent>> GetMatchEventsAsync(string matchId);
    Task<OptaTeamStats> GetTeamStatsAsync(int teamId, string season);
}

public class OptaMatchData
{
    public string MatchId { get; set; } = string.Empty;
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public DateTime MatchDate { get; set; }
    public string Competition { get; set; } = string.Empty;
    public Dictionary<string, object> Statistics { get; set; } = new();
}

public class OptaPlayerStats
{
    public int PlayerId { get; set; }
    public string Season { get; set; } = string.Empty;
    public int Appearances { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int MinutesPlayed { get; set; }
    public Dictionary<string, double> AdvancedStats { get; set; } = new();
}

public class OptaEvent
{
    public string EventId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Minute { get; set; }
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public Dictionary<string, object> Qualifiers { get; set; } = new();
}

public class OptaTeamStats
{
    public int TeamId { get; set; }
    public string Season { get; set; } = string.Empty;
    public int MatchesPlayed { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public Dictionary<string, double> AdvancedStats { get; set; } = new();
}

/// <summary>
/// Wearable device integration service
/// </summary>
public interface IWearableIntegrationService
{
    Task<List<WearableDevice>> GetConnectedDevicesAsync(int playerId);
    Task<bool> ConnectDeviceAsync(int playerId, WearableDevice device);
    Task<bool> DisconnectDeviceAsync(string deviceId);
    Task<WearableData> GetLatestDataAsync(string deviceId);
    Task<List<WearableData>> GetHistoricalDataAsync(string deviceId, DateTime from, DateTime to);
}

public class WearableDevice
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int PlayerId { get; set; }
    public WearableDeviceType Type { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    public DateTime? LastSync { get; set; }
}

public enum WearableDeviceType
{
    GPSTracker,
    HeartRateMonitor,
    SmartWatch,
    FitnessTracker,
    SmartClothing,
    RecoveryDevice
}

public class WearableData
{
    public string DeviceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int? HeartRate { get; set; }
    public double? Distance { get; set; }
    public double? Speed { get; set; }
    public double? Acceleration { get; set; }
    public int? Steps { get; set; }
    public int? Calories { get; set; }
    public Dictionary<string, object>? CustomMetrics { get; set; }
}

/// <summary>
/// Social media integration service
/// </summary>
public interface ISocialMediaIntegrationService
{
    Task<SocialMediaProfile> GetPlayerProfileAsync(int playerId, SocialPlatform platform);
    Task<List<SocialMediaPost>> GetRecentPostsAsync(int playerId, SocialPlatform platform, int count = 10);
    Task<SocialMediaAnalytics> GetAnalyticsAsync(int playerId, SocialPlatform platform);
}

public enum SocialPlatform
{
    Twitter,
    Instagram,
    Facebook,
    TikTok,
    YouTube
}

public class SocialMediaProfile
{
    public int PlayerId { get; set; }
    public SocialPlatform Platform { get; set; }
    public string Username { get; set; } = string.Empty;
    public string ProfileUrl { get; set; } = string.Empty;
    public int Followers { get; set; }
    public int Following { get; set; }
    public bool IsVerified { get; set; }
}

public class SocialMediaPost
{
    public string Id { get; set; } = string.Empty;
    public SocialPlatform Platform { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime PostedAt { get; set; }
    public int Likes { get; set; }
    public int Comments { get; set; }
    public int Shares { get; set; }
}

public class SocialMediaAnalytics
{
    public int PlayerId { get; set; }
    public SocialPlatform Platform { get; set; }
    public int TotalPosts { get; set; }
    public double AverageEngagement { get; set; }
    public int FollowerGrowth { get; set; }
    public Dictionary<string, int> TopHashtags { get; set; } = new();
}

