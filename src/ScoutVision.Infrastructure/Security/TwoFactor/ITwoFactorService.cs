using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScoutVision.Infrastructure.Security.TwoFactor;

/// <summary>
/// Two-Factor Authentication service
/// </summary>
public interface ITwoFactorService
{
    // TOTP (Time-based One-Time Password)
    Task<TwoFactorSetupResult> SetupTOTPAsync(string userId);
    Task<bool> VerifyTOTPAsync(string userId, string code);
    Task<bool> DisableTOTPAsync(string userId, string password);
    
    // SMS-based 2FA
    Task<bool> SendSMSCodeAsync(string userId, string phoneNumber);
    Task<bool> VerifySMSCodeAsync(string userId, string code);
    
    // Email-based 2FA
    Task<bool> SendEmailCodeAsync(string userId, string email);
    Task<bool> VerifyEmailCodeAsync(string userId, string code);
    
    // Backup codes
    Task<List<string>> GenerateBackupCodesAsync(string userId);
    Task<bool> VerifyBackupCodeAsync(string userId, string code);
    Task<List<string>> RegenerateBackupCodesAsync(string userId);
    
    // 2FA status and management
    Task<TwoFactorStatus> GetTwoFactorStatusAsync(string userId);
    Task<bool> IsTwoFactorEnabledAsync(string userId);
    Task<List<TwoFactorMethod>> GetEnabledMethodsAsync(string userId);
    Task<bool> SetPreferredMethodAsync(string userId, TwoFactorMethod method);
}

public class TwoFactorSetupResult
{
    public string Secret { get; set; } = string.Empty;
    public string QRCodeUrl { get; set; } = string.Empty;
    public string ManualEntryKey { get; set; } = string.Empty;
    public List<string> BackupCodes { get; set; } = new();
}

public class TwoFactorStatus
{
    public bool IsEnabled { get; set; }
    public TwoFactorMethod? PreferredMethod { get; set; }
    public List<TwoFactorMethod> EnabledMethods { get; set; } = new();
    public int RemainingBackupCodes { get; set; }
    public DateTime? LastVerified { get; set; }
    public bool RequiresSetup { get; set; }
}

public enum TwoFactorMethod
{
    TOTP,           // Authenticator app (Google Authenticator, Authy, etc.)
    SMS,            // SMS text message
    Email,          // Email code
    BackupCode,     // One-time backup codes
    WebAuthn,       // Hardware security keys (future)
    Biometric       // Fingerprint/Face ID (future)
}

/// <summary>
/// Service for managing trusted devices
/// </summary>
public interface ITrustedDeviceService
{
    Task<string> RegisterTrustedDeviceAsync(string userId, DeviceInfo device);
    Task<bool> IsTrustedDeviceAsync(string userId, string deviceId);
    Task<List<TrustedDevice>> GetTrustedDevicesAsync(string userId);
    Task<bool> RevokeTrustedDeviceAsync(string userId, string deviceId);
    Task RevokeAllTrustedDevicesAsync(string userId);
}

public class DeviceInfo
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty; // Mobile, Desktop, Tablet
    public string Browser { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string? Location { get; set; }
}

public class TrustedDevice
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public DeviceInfo DeviceInfo { get; set; } = new();
    public DateTime TrustedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsed { get; set; }
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(30);
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Service for security event logging
/// </summary>
public interface ISecurityAuditService
{
    Task LogLoginAttemptAsync(string userId, bool success, string ipAddress, DeviceInfo device);
    Task LogTwoFactorAttemptAsync(string userId, TwoFactorMethod method, bool success);
    Task LogPasswordChangeAsync(string userId, string ipAddress);
    Task LogSecuritySettingChangeAsync(string userId, string setting, string oldValue, string newValue);
    Task<List<SecurityEvent>> GetSecurityEventsAsync(string userId, int page = 1, int pageSize = 20);
    Task<List<SuspiciousActivity>> GetSuspiciousActivitiesAsync(string userId);
}

public class SecurityEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public SecurityEventType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DeviceInfo? DeviceInfo { get; set; }
    public bool Success { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string>? Metadata { get; set; }
}

public enum SecurityEventType
{
    Login,
    Logout,
    LoginFailed,
    TwoFactorEnabled,
    TwoFactorDisabled,
    TwoFactorVerified,
    TwoFactorFailed,
    PasswordChanged,
    PasswordResetRequested,
    PasswordResetCompleted,
    EmailChanged,
    PhoneChanged,
    TrustedDeviceAdded,
    TrustedDeviceRevoked,
    AccountLocked,
    AccountUnlocked,
    SuspiciousActivity,
    SecuritySettingChanged
}

public class SuspiciousActivity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public SuspiciousActivityType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public int RiskScore { get; set; } // 0-100
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public bool IsResolved { get; set; }
    public string? Resolution { get; set; }
}

public enum SuspiciousActivityType
{
    MultipleFailedLogins,
    LoginFromNewLocation,
    LoginFromNewDevice,
    UnusualAccessPattern,
    RapidPasswordChanges,
    MultipleAccountAccess,
    DataExfiltration,
    APIAbuse
}

/// <summary>
/// Service for account security recommendations
/// </summary>
public interface ISecurityRecommendationService
{
    Task<List<SecurityRecommendation>> GetRecommendationsAsync(string userId);
    Task<SecurityScore> CalculateSecurityScoreAsync(string userId);
    Task<bool> DismissRecommendationAsync(string userId, string recommendationId);
}

public class SecurityRecommendation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public SecurityRecommendationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SecurityRecommendationPriority Priority { get; set; }
    public string? ActionUrl { get; set; }
    public bool IsDismissed { get; set; }
}

public enum SecurityRecommendationType
{
    EnableTwoFactor,
    UseStrongerPassword,
    ReviewTrustedDevices,
    UpdateRecoveryEmail,
    UpdateRecoveryPhone,
    ReviewRecentActivity,
    EnableLoginAlerts,
    ReviewAPIKeys,
    UpdateSecurityQuestions
}

public enum SecurityRecommendationPriority
{
    Low,
    Medium,
    High,
    Critical
}

public class SecurityScore
{
    public int Score { get; set; } // 0-100
    public string Level { get; set; } = string.Empty; // Weak, Fair, Good, Strong, Excellent
    public List<SecurityFactor> Factors { get; set; } = new();
    public List<SecurityRecommendation> Recommendations { get; set; } = new();
}

public class SecurityFactor
{
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public int Points { get; set; }
    public string Description { get; set; } = string.Empty;
}

