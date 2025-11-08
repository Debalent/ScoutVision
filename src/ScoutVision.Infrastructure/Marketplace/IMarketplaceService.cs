using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScoutVision.Infrastructure.Marketplace;

/// <summary>
/// API Marketplace for third-party integrations and plugins
/// </summary>
public interface IMarketplaceService
{
    // Plugin management
    Task<List<Plugin>> GetAvailablePluginsAsync(PluginCategory? category = null);
    Task<Plugin> GetPluginAsync(string pluginId);
    Task<List<Plugin>> GetInstalledPluginsAsync(string organizationId);
    Task<bool> InstallPluginAsync(string organizationId, string pluginId);
    Task<bool> UninstallPluginAsync(string organizationId, string pluginId);
    Task<bool> UpdatePluginAsync(string organizationId, string pluginId);
    
    // Plugin configuration
    Task<PluginConfiguration> GetPluginConfigurationAsync(string organizationId, string pluginId);
    Task<bool> UpdatePluginConfigurationAsync(string organizationId, string pluginId, Dictionary<string, object> settings);
    
    // Plugin marketplace
    Task<string> PublishPluginAsync(Plugin plugin);
    Task<bool> UpdatePluginListingAsync(Plugin plugin);
    Task<bool> UnpublishPluginAsync(string pluginId);
    
    // Reviews and ratings
    Task<bool> AddReviewAsync(string pluginId, PluginReview review);
    Task<List<PluginReview>> GetPluginReviewsAsync(string pluginId, int page = 1, int pageSize = 20);
    Task<PluginRating> GetPluginRatingAsync(string pluginId);
    
    // Analytics
    Task<PluginAnalytics> GetPluginAnalyticsAsync(string pluginId);
    Task<MarketplaceStatistics> GetMarketplaceStatisticsAsync();
}

public class Plugin
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public PluginCategory Category { get; set; }
    public string DeveloperId { get; set; } = string.Empty;
    public string DeveloperName { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public List<string> Screenshots { get; set; } = new();
    public PluginPricing Pricing { get; set; } = new();
    public List<string> Features { get; set; } = new();
    public List<PluginPermission> RequiredPermissions { get; set; } = new();
    public PluginStatus Status { get; set; } = PluginStatus.Draft;
    public int InstallCount { get; set; }
    public PluginRating Rating { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
}

public enum PluginCategory
{
    DataIntegration,
    Analytics,
    Reporting,
    VideoAnalysis,
    InjuryPrevention,
    TransferMarket,
    Collaboration,
    Automation,
    CustomDashboards,
    AIModels,
    Notifications,
    Security,
    Other
}

public class PluginPricing
{
    public PricingModel Model { get; set; } = PricingModel.Free;
    public decimal? MonthlyPrice { get; set; }
    public decimal? AnnualPrice { get; set; }
    public decimal? OneTimePrice { get; set; }
    public bool HasFreeTrial { get; set; }
    public int? FreeTrialDays { get; set; }
    public List<PricingTier>? Tiers { get; set; }
}

public enum PricingModel
{
    Free,
    Freemium,
    Subscription,
    OneTime,
    UsageBased,
    Custom
}

public class PricingTier
{
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }
    public List<string> Features { get; set; } = new();
    public Dictionary<string, int>? Limits { get; set; }
}

public enum PluginPermission
{
    ReadPlayerData,
    WritePlayerData,
    ReadTeamData,
    WriteTeamData,
    ReadMatchData,
    WriteMatchData,
    AccessVideoAnalysis,
    AccessInjuryData,
    AccessTransferData,
    SendNotifications,
    AccessUserData,
    ManageWorkspaces,
    ExportData,
    ImportData,
    AccessAPI,
    ManageBilling
}

public enum PluginStatus
{
    Draft,
    UnderReview,
    Approved,
    Published,
    Suspended,
    Deprecated
}

public class PluginConfiguration
{
    public string PluginId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, object> Settings { get; set; } = new();
    public DateTime InstalledAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdated { get; set; }
}

public class PluginReview
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PluginId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5 stars
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Pros { get; set; } = new();
    public List<string> Cons { get; set; } = new();
    public bool IsVerifiedPurchase { get; set; }
    public int HelpfulCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class PluginRating
{
    public string PluginId { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new(); // Star -> Count
}

public class PluginAnalytics
{
    public string PluginId { get; set; } = string.Empty;
    public int TotalInstalls { get; set; }
    public int ActiveInstalls { get; set; }
    public int UninstallCount { get; set; }
    public Dictionary<string, int> InstallsByCountry { get; set; } = new();
    public Dictionary<string, int> InstallsByPlan { get; set; } = new();
    public List<PluginUsageMetric> UsageMetrics { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRecurringRevenue { get; set; }
}

public class PluginUsageMetric
{
    public DateTime Date { get; set; }
    public int ActiveUsers { get; set; }
    public int ApiCalls { get; set; }
    public TimeSpan AverageSessionDuration { get; set; }
}

public class MarketplaceStatistics
{
    public int TotalPlugins { get; set; }
    public int PublishedPlugins { get; set; }
    public int TotalDevelopers { get; set; }
    public int TotalInstalls { get; set; }
    public Dictionary<PluginCategory, int> PluginsByCategory { get; set; } = new();
    public List<Plugin> FeaturedPlugins { get; set; } = new();
    public List<Plugin> TrendingPlugins { get; set; } = new();
    public List<Plugin> NewPlugins { get; set; } = new();
}

/// <summary>
/// Service for plugin webhooks and events
/// </summary>
public interface IPluginWebhookService
{
    Task<string> RegisterWebhookAsync(string pluginId, WebhookRegistration registration);
    Task<bool> UnregisterWebhookAsync(string webhookId);
    Task<List<WebhookRegistration>> GetPluginWebhooksAsync(string pluginId);
    Task TriggerWebhookAsync(string pluginId, string eventType, object payload);
    Task<List<WebhookDelivery>> GetWebhookDeliveriesAsync(string webhookId);
}

public class WebhookRegistration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PluginId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public List<string> Events { get; set; } = new();
    public string? Secret { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class WebhookDelivery
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string WebhookId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public object Payload { get; set; } = new();
    public int StatusCode { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime DeliveredAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ResponseTime { get; set; }
}

/// <summary>
/// Service for plugin API key management
/// </summary>
public interface IPluginApiKeyService
{
    Task<ApiKey> GenerateApiKeyAsync(string pluginId, string organizationId, ApiKeyScope scope);
    Task<bool> RevokeApiKeyAsync(string apiKeyId);
    Task<List<ApiKey>> GetPluginApiKeysAsync(string pluginId, string organizationId);
    Task<bool> ValidateApiKeyAsync(string key);
    Task<ApiKeyUsage> GetApiKeyUsageAsync(string apiKeyId);
}

public class ApiKey
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PluginId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ApiKeyScope Scope { get; set; }
    public List<string> AllowedEndpoints { get; set; } = new();
    public int? RateLimit { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsed { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum ApiKeyScope
{
    ReadOnly,
    ReadWrite,
    Admin,
    Custom
}

public class ApiKeyUsage
{
    public string ApiKeyId { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int RequestsToday { get; set; }
    public int RequestsThisMonth { get; set; }
    public Dictionary<string, int> RequestsByEndpoint { get; set; } = new();
    public DateTime? LastRequest { get; set; }
}

/// <summary>
/// Service for revenue sharing with plugin developers
/// </summary>
public interface IPluginRevenueService
{
    Task<PluginRevenue> GetPluginRevenueAsync(string pluginId, DateTime from, DateTime to);
    Task<List<RevenueTransaction>> GetRevenueTransactionsAsync(string pluginId);
    Task<PayoutSummary> GetPayoutSummaryAsync(string developerId);
    Task<bool> RequestPayoutAsync(string developerId);
}

public class PluginRevenue
{
    public string PluginId { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public decimal DeveloperShare { get; set; }
    public decimal PlatformFee { get; set; }
    public int TotalSales { get; set; }
    public int ActiveSubscriptions { get; set; }
    public decimal MonthlyRecurringRevenue { get; set; }
    public List<RevenueTransaction> Transactions { get; set; } = new();
}

public class RevenueTransaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PluginId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal DeveloperShare { get; set; }
    public decimal PlatformFee { get; set; }
    public TransactionType Type { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
}

public enum TransactionType
{
    Purchase,
    Subscription,
    Renewal,
    Refund,
    Chargeback
}

public class PayoutSummary
{
    public string DeveloperId { get; set; } = string.Empty;
    public decimal TotalEarnings { get; set; }
    public decimal PaidOut { get; set; }
    public decimal PendingPayout { get; set; }
    public decimal MinimumPayout { get; set; } = 100;
    public bool CanRequestPayout => PendingPayout >= MinimumPayout;
    public DateTime? NextPayoutDate { get; set; }
    public List<Payout> PayoutHistory { get; set; } = new();
}

public class Payout
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DeveloperId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PayoutStatus Status { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}

public enum PayoutStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled
}

