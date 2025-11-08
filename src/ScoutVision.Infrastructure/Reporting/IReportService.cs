using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ScoutVision.Infrastructure.Reporting;

/// <summary>
/// Comprehensive reporting service for generating professional scouting reports
/// </summary>
public interface IReportService
{
    // PDF Reports
    Task<byte[]> GeneratePlayerReportPdfAsync(int playerId, ReportOptions options);
    Task<byte[]> GenerateTeamReportPdfAsync(int teamId, ReportOptions options);
    Task<byte[]> GenerateInjuryReportPdfAsync(int clubId, DateTime from, DateTime to);
    Task<byte[]> GenerateTransferReportPdfAsync(int clubId, ReportOptions options);
    Task<byte[]> GenerateCustomReportPdfAsync(CustomReportRequest request);
    
    // Excel Reports
    Task<byte[]> GeneratePlayerStatsExcelAsync(List<int> playerIds, ExcelReportOptions options);
    Task<byte[]> GenerateTransferMarketExcelAsync(TransferMarketFilter filter);
    Task<byte[]> GenerateInjuryAnalyticsExcelAsync(int clubId, DateTime from, DateTime to);
    
    // PowerPoint Presentations
    Task<byte[]> GenerateScoutingPresentationAsync(int playerId, PresentationOptions options);
    Task<byte[]> GenerateSeasonReviewPresentationAsync(int teamId, string season);
    
    // Report Templates
    Task<List<ReportTemplate>> GetAvailableTemplatesAsync(ReportType type);
    Task<ReportTemplate> GetTemplateAsync(string templateId);
    Task<string> CreateCustomTemplateAsync(ReportTemplate template);
    Task UpdateTemplateAsync(ReportTemplate template);
    Task DeleteTemplateAsync(string templateId);
    
    // Scheduled Reports
    Task<string> ScheduleReportAsync(ScheduledReport report);
    Task<List<ScheduledReport>> GetScheduledReportsAsync(string userId);
    Task UpdateScheduledReportAsync(ScheduledReport report);
    Task CancelScheduledReportAsync(string reportId);
    
    // Report History
    Task<List<GeneratedReport>> GetReportHistoryAsync(string userId, int page = 1, int pageSize = 20);
    Task<byte[]> DownloadHistoricalReportAsync(string reportId);
    Task DeleteHistoricalReportAsync(string reportId);
}

public class ReportOptions
{
    public string TemplateId { get; set; } = "default";
    public bool IncludeStatistics { get; set; } = true;
    public bool IncludeCharts { get; set; } = true;
    public bool IncludeHeatMaps { get; set; } = true;
    public bool IncludeVideoHighlights { get; set; } = false;
    public bool IncludeComparisons { get; set; } = true;
    public bool IncludeInjuryHistory { get; set; } = true;
    public bool IncludeTransferHistory { get; set; } = true;
    public string? Season { get; set; }
    public string? Competition { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Watermark { get; set; }
    public BrandingOptions? Branding { get; set; }
    public List<string>? CustomSections { get; set; }
}

public class BrandingOptions
{
    public string? ClubName { get; set; }
    public byte[]? ClubLogo { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? FooterText { get; set; }
}

public class ExcelReportOptions
{
    public bool IncludeCharts { get; set; } = true;
    public bool IncludePivotTables { get; set; } = false;
    public bool AutoFilter { get; set; } = true;
    public bool FreezeHeaders { get; set; } = true;
    public List<string>? Columns { get; set; }
    public string? SheetName { get; set; }
}

public class PresentationOptions
{
    public string TemplateId { get; set; } = "default";
    public int MaxSlides { get; set; } = 20;
    public bool IncludeExecutiveSummary { get; set; } = true;
    public bool IncludeVideoClips { get; set; } = false;
    public bool IncludeStatistics { get; set; } = true;
    public bool IncludeComparisons { get; set; } = true;
    public BrandingOptions? Branding { get; set; }
}

public class CustomReportRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<ReportSection> Sections { get; set; } = new();
    public ReportOptions Options { get; set; } = new();
}

public class ReportSection
{
    public string Title { get; set; } = string.Empty;
    public ReportSectionType Type { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public int Order { get; set; }
}

public enum ReportSectionType
{
    Text,
    Statistics,
    Chart,
    Table,
    HeatMap,
    PlayerComparison,
    VideoHighlights,
    Timeline,
    RadarChart,
    PerformanceTrend,
    InjuryHistory,
    TransferHistory,
    TacticalAnalysis,
    CustomHtml
}

public class ReportTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ReportType Type { get; set; }
    public List<ReportSection> Sections { get; set; } = new();
    public string? ThumbnailUrl { get; set; }
    public bool IsPublic { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum ReportType
{
    PlayerScouting,
    TeamAnalysis,
    InjuryReport,
    TransferMarket,
    SeasonReview,
    MatchAnalysis,
    Custom
}

public class ScheduledReport
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ReportType Type { get; set; }
    public string TemplateId { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public ReportSchedule Schedule { get; set; } = new();
    public List<string> Recipients { get; set; } = new();
    public ReportFormat Format { get; set; } = ReportFormat.PDF;
    public bool IsActive { get; set; } = true;
    public DateTime? LastRun { get; set; }
    public DateTime? NextRun { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ReportSchedule
{
    public ScheduleFrequency Frequency { get; set; }
    public int Interval { get; set; } = 1;
    public DayOfWeek? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public TimeSpan Time { get; set; } = new TimeSpan(9, 0, 0);
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public enum ScheduleFrequency
{
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Annually
}

public enum ReportFormat
{
    PDF,
    Excel,
    PowerPoint,
    HTML,
    JSON
}

public class GeneratedReport
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ReportType Type { get; set; }
    public ReportFormat Format { get; set; }
    public long FileSizeBytes { get; set; }
    public string? StorageUrl { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public int DownloadCount { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class TransferMarketFilter
{
    public string? Position { get; set; }
    public string? League { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public string? Nationality { get; set; }
    public bool? ContractExpiring { get; set; }
    public List<string>? Clubs { get; set; }
}

/// <summary>
/// Service for report analytics and insights
/// </summary>
public interface IReportAnalyticsService
{
    Task<ReportUsageStats> GetReportUsageStatsAsync(string userId, DateTime from, DateTime to);
    Task<List<PopularReport>> GetMostGeneratedReportsAsync(int limit = 10);
    Task<Dictionary<ReportType, int>> GetReportTypeBreakdownAsync(string userId);
    Task TrackReportGenerationAsync(string reportId, string userId, ReportType type);
    Task TrackReportDownloadAsync(string reportId);
}

public class ReportUsageStats
{
    public int TotalReportsGenerated { get; set; }
    public int TotalDownloads { get; set; }
    public long TotalStorageUsedBytes { get; set; }
    public Dictionary<ReportType, int> ByType { get; set; } = new();
    public Dictionary<ReportFormat, int> ByFormat { get; set; } = new();
    public List<GeneratedReport> RecentReports { get; set; } = new();
}

public class PopularReport
{
    public string TemplateId { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public int GenerationCount { get; set; }
    public int DownloadCount { get; set; }
}

