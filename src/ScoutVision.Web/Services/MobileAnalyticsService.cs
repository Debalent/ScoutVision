using ScoutVision.Web.Models;

namespace ScoutVision.Web.Services;

public class MobileAnalyticsService : IMobileAnalyticsService
{
    private readonly IHybridAnalyticsService _analyticsService;
    private readonly ILogger<MobileAnalyticsService> _logger;
    private readonly Dictionary<string, string> _mobileDevices = new();

    public MobileAnalyticsService(IHybridAnalyticsService analyticsService, ILogger<MobileAnalyticsService> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<MobileDashboardData> GetDashboardDataAsync()
    {
        try
        {
            var quickStats = await GetQuickStatsAsync();
            var topPlayers = await GetTopPlayersAsync(5);
            var recentActivities = await GetRecentActivitiesAsync(10);

            return new MobileDashboardData
            {
                QuickStats = quickStats,
                TopPlayers = topPlayers,
                RecentActivities = recentActivities,
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get mobile dashboard data");
            return new MobileDashboardData();
        }
    }

    public async Task<List<QuickStat>> GetQuickStatsAsync()
    {
        // Simulate real data - replace with actual service calls
        await Task.Delay(100);
        
        return new List<QuickStat>
        {
            new() { Title = "Active Sessions", Value = "12", Icon = "activity", Color = "success" },
            new() { Title = "Players Analyzed", Value = "847", Icon = "people", Color = "primary" },
            new() { Title = "Injury Alerts", Value = "3", Icon = "exclamation-triangle", Color = "warning" },
            new() { Title = "Live Matches", Value = "5", Icon = "broadcast", Color = "info" }
        };
    }

    public async Task RegisterMobileDeviceAsync(string deviceId, string connectionId)
    {
        _mobileDevices[deviceId] = connectionId;
        _logger.LogInformation($"Registered mobile device {deviceId} with connection {connectionId}");
        await Task.CompletedTask;
    }

    public async Task<List<PlayerSummary>> GetTopPlayersAsync(int count = 10)
    {
        await Task.Delay(50);
        
        return Enumerable.Range(1, count).Select(i => new PlayerSummary
        {
            Id = i,
            Name = $"Player {i}",
            Position = i % 4 switch { 0 => "GK", 1 => "DEF", 2 => "MID", _ => "FWD" },
            OverallRating = 75 + (i * 2),
            RecentForm = Random.Shared.Next(60, 95)
        }).ToList();
    }

    public async Task<List<RecentActivity>> GetRecentActivitiesAsync(int count = 20)
    {
        await Task.Delay(50);
        
        return Enumerable.Range(1, count).Select(i => new RecentActivity
        {
            Id = i,
            Description = $"Activity {i} completed",
            Timestamp = DateTime.UtcNow.AddMinutes(-i * 5),
            Type = i % 3 switch { 0 => "analysis", 1 => "alert", _ => "update" }
        }).ToList();
    }
}