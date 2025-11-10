using ScoutVision.Web.Models;

namespace ScoutVision.Web.Services;

public interface IMobileAnalyticsService
{
    Task<MobileDashboardData> GetDashboardDataAsync();
    Task<List<QuickStat>> GetQuickStatsAsync();
    Task RegisterMobileDeviceAsync(string deviceId, string connectionId);
    Task<List<PlayerSummary>> GetTopPlayersAsync(int count = 10);
    Task<List<RecentActivity>> GetRecentActivitiesAsync(int count = 20);
}