using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoutVision.Infrastructure.Security.TwoFactor;

namespace ScoutVision.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SecurityController : ControllerBase
{
    private readonly ITwoFactorService _twoFactorService;
    private readonly ITrustedDeviceService _trustedDeviceService;
    private readonly ISecurityAuditService _auditService;
    private readonly ISecurityRecommendationService _recommendationService;

    public SecurityController(
        ITwoFactorService twoFactorService,
        ITrustedDeviceService trustedDeviceService,
        ISecurityAuditService auditService,
        ISecurityRecommendationService recommendationService)
    {
        _twoFactorService = twoFactorService;
        _trustedDeviceService = trustedDeviceService;
        _auditService = auditService;
        _recommendationService = recommendationService;
    }

    #region Two-Factor Authentication

    /// <summary>
    /// Setup TOTP (Time-based One-Time Password) authentication
    /// </summary>
    [HttpPost("2fa/totp/setup")]
    public async Task<ActionResult<TwoFactorSetupResult>> SetupTOTP()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var result = await _twoFactorService.SetupTOTPAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Verify TOTP code
    /// </summary>
    [HttpPost("2fa/totp/verify")]
    public async Task<ActionResult<bool>> VerifyTOTP([FromBody] VerifyCodeRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var isValid = await _twoFactorService.VerifyTOTPAsync(userId, request.Code);
        return Ok(new { isValid });
    }

    /// <summary>
    /// Disable TOTP authentication
    /// </summary>
    [HttpPost("2fa/totp/disable")]
    public async Task<IActionResult> DisableTOTP([FromBody] DisableTwoFactorRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var success = await _twoFactorService.DisableTOTPAsync(userId, request.Password);
        
        if (!success)
            return BadRequest(new { message = "Invalid password" });
        
        return NoContent();
    }

    /// <summary>
    /// Send SMS verification code
    /// </summary>
    [HttpPost("2fa/sms/send")]
    public async Task<IActionResult> SendSMSCode([FromBody] SendSMSRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var success = await _twoFactorService.SendSMSCodeAsync(userId, request.PhoneNumber);
        
        if (!success)
            return BadRequest(new { message = "Failed to send SMS code" });
        
        return Ok(new { message = "SMS code sent successfully" });
    }

    /// <summary>
    /// Verify SMS code
    /// </summary>
    [HttpPost("2fa/sms/verify")]
    public async Task<ActionResult<bool>> VerifySMSCode([FromBody] VerifyCodeRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var isValid = await _twoFactorService.VerifySMSCodeAsync(userId, request.Code);
        return Ok(new { isValid });
    }

    /// <summary>
    /// Generate backup codes
    /// </summary>
    [HttpPost("2fa/backup-codes/generate")]
    public async Task<ActionResult<List<string>>> GenerateBackupCodes()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var codes = await _twoFactorService.GenerateBackupCodesAsync(userId);
        return Ok(codes);
    }

    /// <summary>
    /// Verify backup code
    /// </summary>
    [HttpPost("2fa/backup-codes/verify")]
    public async Task<ActionResult<bool>> VerifyBackupCode([FromBody] VerifyCodeRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var isValid = await _twoFactorService.VerifyBackupCodeAsync(userId, request.Code);
        return Ok(new { isValid });
    }

    /// <summary>
    /// Get two-factor authentication status
    /// </summary>
    [HttpGet("2fa/status")]
    public async Task<ActionResult<TwoFactorStatus>> GetTwoFactorStatus()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var status = await _twoFactorService.GetTwoFactorStatusAsync(userId);
        return Ok(status);
    }

    /// <summary>
    /// Set preferred 2FA method
    /// </summary>
    [HttpPut("2fa/preferred-method")]
    public async Task<IActionResult> SetPreferredMethod([FromBody] SetPreferredMethodRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        await _twoFactorService.SetPreferredMethodAsync(userId, request.Method);
        return NoContent();
    }

    #endregion

    #region Trusted Devices

    /// <summary>
    /// Register trusted device
    /// </summary>
    [HttpPost("trusted-devices")]
    public async Task<ActionResult<string>> RegisterTrustedDevice([FromBody] DeviceInfo deviceInfo)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var deviceId = await _trustedDeviceService.RegisterTrustedDeviceAsync(userId, deviceInfo);
        return Ok(new { deviceId });
    }

    /// <summary>
    /// Get trusted devices
    /// </summary>
    [HttpGet("trusted-devices")]
    public async Task<ActionResult<List<TrustedDevice>>> GetTrustedDevices()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var devices = await _trustedDeviceService.GetTrustedDevicesAsync(userId);
        return Ok(devices);
    }

    /// <summary>
    /// Revoke trusted device
    /// </summary>
    [HttpDelete("trusted-devices/{deviceId}")]
    public async Task<IActionResult> RevokeTrustedDevice(string deviceId)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        await _trustedDeviceService.RevokeTrustedDeviceAsync(userId, deviceId);
        return NoContent();
    }

    /// <summary>
    /// Revoke all trusted devices
    /// </summary>
    [HttpDelete("trusted-devices")]
    public async Task<IActionResult> RevokeAllTrustedDevices()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        await _trustedDeviceService.RevokeAllTrustedDevicesAsync(userId);
        return NoContent();
    }

    #endregion

    #region Security Audit

    /// <summary>
    /// Get security events
    /// </summary>
    [HttpGet("audit/events")]
    public async Task<ActionResult<List<SecurityEvent>>> GetSecurityEvents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var events = await _auditService.GetSecurityEventsAsync(userId, page, pageSize);
        return Ok(events);
    }

    /// <summary>
    /// Get suspicious activities
    /// </summary>
    [HttpGet("audit/suspicious")]
    public async Task<ActionResult<List<SuspiciousActivity>>> GetSuspiciousActivities()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var activities = await _auditService.GetSuspiciousActivitiesAsync(userId);
        return Ok(activities);
    }

    #endregion

    #region Security Recommendations

    /// <summary>
    /// Get security recommendations
    /// </summary>
    [HttpGet("recommendations")]
    public async Task<ActionResult<List<SecurityRecommendation>>> GetRecommendations()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var recommendations = await _recommendationService.GetRecommendationsAsync(userId);
        return Ok(recommendations);
    }

    /// <summary>
    /// Get security score
    /// </summary>
    [HttpGet("score")]
    public async Task<ActionResult<SecurityScore>> GetSecurityScore()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var score = await _recommendationService.CalculateSecurityScoreAsync(userId);
        return Ok(score);
    }

    /// <summary>
    /// Dismiss recommendation
    /// </summary>
    [HttpPost("recommendations/{recommendationId}/dismiss")]
    public async Task<IActionResult> DismissRecommendation(string recommendationId)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        await _recommendationService.DismissRecommendationAsync(userId, recommendationId);
        return NoContent();
    }

    #endregion
}

#region Request Models

public class VerifyCodeRequest
{
    public string Code { get; set; } = string.Empty;
}

public class DisableTwoFactorRequest
{
    public string Password { get; set; } = string.Empty;
}

public class SendSMSRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
}

public class SetPreferredMethodRequest
{
    public TwoFactorMethod Method { get; set; }
}

#endregion

