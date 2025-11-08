using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScoutVision.Infrastructure.Notifications;

/// <summary>
/// Comprehensive notification service for multi-channel alerts
/// </summary>
public interface INotificationService
{
    // In-app notifications
    Task SendInAppNotificationAsync(string userId, Notification notification);
    Task<List<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20);
    Task MarkAsReadAsync(string userId, string notificationId);
    Task MarkAllAsReadAsync(string userId);
    Task DeleteNotificationAsync(string userId, string notificationId);
    
    // Email notifications
    Task SendEmailNotificationAsync(string email, EmailNotification notification);
    Task SendBulkEmailAsync(List<string> emails, EmailNotification notification);
    
    // SMS notifications
    Task SendSMSNotificationAsync(string phoneNumber, string message);
    
    // Push notifications
    Task SendPushNotificationAsync(string userId, PushNotification notification);
    Task SendPushToDeviceAsync(string deviceToken, PushNotification notification);
    
    // Scheduled notifications
    Task ScheduleNotificationAsync(string userId, Notification notification, DateTime scheduledTime);
    Task CancelScheduledNotificationAsync(string notificationId);
    
    // Notification preferences
    Task<NotificationPreferences> GetUserPreferencesAsync(string userId);
    Task UpdateUserPreferencesAsync(string userId, NotificationPreferences preferences);
}

public class Notification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? IconUrl { get; set; }
    public Dictionary<string, object>? Data { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class EmailNotification
{
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? PlainTextBody { get; set; }
    public List<EmailAttachment>? Attachments { get; set; }
    public string? ReplyTo { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
}

public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
}

public class PushNotification
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Badge { get; set; }
    public string? Image { get; set; }
    public string? ClickAction { get; set; }
    public Dictionary<string, string>? Data { get; set; }
    public List<NotificationAction>? Actions { get; set; }
}

public class NotificationAction
{
    public string Action { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Icon { get; set; }
}

public class NotificationPreferences
{
    public string UserId { get; set; } = string.Empty;
    
    // Channel preferences
    public bool EnableInApp { get; set; } = true;
    public bool EnableEmail { get; set; } = true;
    public bool EnableSMS { get; set; } = false;
    public bool EnablePush { get; set; } = true;
    
    // Type-specific preferences
    public Dictionary<NotificationType, ChannelPreference> TypePreferences { get; set; } = new();
    
    // Quiet hours
    public TimeSpan? QuietHoursStart { get; set; }
    public TimeSpan? QuietHoursEnd { get; set; }
    public List<DayOfWeek>? QuietDays { get; set; }
    
    // Digest preferences
    public bool EnableDailyDigest { get; set; } = false;
    public bool EnableWeeklyDigest { get; set; } = false;
    public TimeSpan? DigestTime { get; set; }
    
    // Frequency limits
    public int? MaxNotificationsPerHour { get; set; }
    public int? MaxEmailsPerDay { get; set; }
}

public class ChannelPreference
{
    public bool InApp { get; set; } = true;
    public bool Email { get; set; } = true;
    public bool SMS { get; set; } = false;
    public bool Push { get; set; } = true;
}

public enum NotificationType
{
    InjuryAlert,
    TransferOpportunity,
    PlayerUpdate,
    MatchAnalysis,
    SystemAlert,
    TeamInvitation,
    ReportReady,
    CommentMention,
    TaskAssignment,
    SubscriptionExpiring,
    PaymentFailed,
    SecurityAlert,
    FeatureAnnouncement,
    MaintenanceNotice
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Urgent
}

/// <summary>
/// Service for managing notification templates
/// </summary>
public interface INotificationTemplateService
{
    Task<string> RenderTemplateAsync(string templateName, object data);
    Task<EmailNotification> GetEmailTemplateAsync(string templateName, object data);
    Task CreateTemplateAsync(NotificationTemplate template);
    Task UpdateTemplateAsync(NotificationTemplate template);
    Task DeleteTemplateAsync(string templateId);
}

public class NotificationTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlTemplate { get; set; } = string.Empty;
    public string? PlainTextTemplate { get; set; }
    public Dictionary<string, string>? DefaultValues { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Service for notification analytics and tracking
/// </summary>
public interface INotificationAnalyticsService
{
    Task TrackNotificationSentAsync(string notificationId, NotificationType type, string channel);
    Task TrackNotificationDeliveredAsync(string notificationId);
    Task TrackNotificationReadAsync(string notificationId);
    Task TrackNotificationClickedAsync(string notificationId);
    Task<NotificationStats> GetNotificationStatsAsync(string userId, DateTime from, DateTime to);
    Task<Dictionary<NotificationType, int>> GetNotificationBreakdownAsync(string userId);
}

public class NotificationStats
{
    public int TotalSent { get; set; }
    public int TotalDelivered { get; set; }
    public int TotalRead { get; set; }
    public int TotalClicked { get; set; }
    public decimal DeliveryRate => TotalSent > 0 ? (decimal)TotalDelivered / TotalSent * 100 : 0;
    public decimal ReadRate => TotalDelivered > 0 ? (decimal)TotalRead / TotalDelivered * 100 : 0;
    public decimal ClickRate => TotalRead > 0 ? (decimal)TotalClicked / TotalRead * 100 : 0;
    public Dictionary<string, int> ByChannel { get; set; } = new();
    public Dictionary<NotificationType, int> ByType { get; set; } = new();
}

/// <summary>
/// Real-time notification hub for SignalR
/// </summary>
public interface INotificationHub
{
    Task SendNotificationToUser(string userId, Notification notification);
    Task SendNotificationToGroup(string groupId, Notification notification);
    Task BroadcastNotification(Notification notification);
}

