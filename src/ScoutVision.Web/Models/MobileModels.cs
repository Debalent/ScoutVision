namespace ScoutVision.Web.Models;

public class MobileDashboardData
{
    public List<QuickStat> QuickStats { get; set; } = new();
    public List<PlayerSummary> TopPlayers { get; set; } = new();
    public List<RecentActivity> RecentActivities { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class QuickStat
{
    public string Title { get; set; } = "";
    public string Value { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Color { get; set; } = "primary";
}

public class PlayerSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Position { get; set; } = "";
    public int OverallRating { get; set; }
    public int RecentForm { get; set; }
}

public class RecentActivity
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string Type { get; set; } = "";
}