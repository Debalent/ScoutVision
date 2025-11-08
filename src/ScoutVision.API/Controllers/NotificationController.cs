using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoutVision.Infrastructure.Notifications;

namespace ScoutVision.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly INotificationAnalyticsService _analyticsService;

    public NotificationController(
        INotificationService notificationService,
        INotificationAnalyticsService analyticsService)
    {
        _notificationService = notificationService;
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Get user notifications with pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<Notification>>> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize);
        return Ok(notifications);
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    [HttpPut("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(string notificationId)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        await _notificationService.MarkAsReadAsync(userId, notificationId);
        return NoContent();
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        await _notificationService.MarkAllAsReadAsync(userId);
        return NoContent();
    }

    /// <summary>
    /// Delete a notification
    /// </summary>
    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> DeleteNotification(string notificationId)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        await _notificationService.DeleteNotificationAsync(userId, notificationId);
        return NoContent();
    }

    /// <summary>
    /// Get notification preferences
    /// </summary>
    [HttpGet("preferences")]
    public async Task<ActionResult<NotificationPreferences>> GetPreferences()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var preferences = await _notificationService.GetUserPreferencesAsync(userId);
        return Ok(preferences);
    }

    /// <summary>
    /// Update notification preferences
    /// </summary>
    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] NotificationPreferences preferences)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        await _notificationService.UpdateUserPreferencesAsync(userId, preferences);
        return NoContent();
    }

    /// <summary>
    /// Get notification statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<NotificationStats>> GetStatistics(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = to ?? DateTime.UtcNow;
        
        var stats = await _analyticsService.GetNotificationStatsAsync(userId, fromDate, toDate);
        return Ok(stats);
    }

    /// <summary>
    /// Send a test notification
    /// </summary>
    [HttpPost("test")]
    public async Task<IActionResult> SendTestNotification()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        
        var notification = new Notification
        {
            UserId = userId,
            Type = NotificationType.SystemAlert,
            Title = "Test Notification",
            Message = "This is a test notification from ScoutVision Pro",
            Priority = NotificationPriority.Normal
        };

        await _notificationService.SendInAppNotificationAsync(userId, notification);
        return Ok(new { message = "Test notification sent successfully" });
    }
}

