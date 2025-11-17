using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using ScoutVision.Infrastructure.Caching;

namespace ScoutVision.Infrastructure.Services;

/// <summary>
/// Service for multi-tenant support
/// Enables ScoutVision to serve multiple clubs/organizations
/// with data isolation and customizable packages
/// </summary>
public interface IMultiTenantService
{
    string GetCurrentTenantId();
    Task<TenantInfo> GetTenantInfoAsync(string tenantId);
    Task<bool> IsTenantActiveAsync(string tenantId);
    Task<TenantPackage> GetTenantPackageAsync(string tenantId);
    Task<List<TenantUser>> GetTenantUsersAsync(string tenantId);
    Task AddTenantUserAsync(string tenantId, TenantUser user);
    Task RemoveTenantUserAsync(string tenantId, string userId);
}

public class TenantInfo
{
    public required string TenantId { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; } // "Club", "Agency", "Sportsbook", "HighSchool"
    public required string Country { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public required TenantPackage Package { get; set; }
    public required Dictionary<string, object> CustomSettings { get; set; }
}

public class TenantPackage
{
    public required string PackageName { get; set; } // "Scout", "Coach", "Enterprise", "HighSchool"
    public required List<string> IncludedModules { get; set; }
    public int MaxUsers { get; set; }
    public int MaxPlayers { get; set; }
    public bool IncludesInjuryPrevention { get; set; }
    public bool IncludesTransferValuation { get; set; }
    public bool IncludesHighSchoolPackage { get; set; }
    public bool IncludesCoachingFeedback { get; set; }
    public int ApiCallsPerMonth { get; set; }
    public bool RealtimeDataAccess { get; set; }
    public decimal MonthlyPrice { get; set; }
}

public class TenantUser
{
    public required string UserId { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public required string Role { get; set; } // "Admin", "Coach", "Analyst", "Viewer"
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Implementation of multi-tenant service
/// </summary>
public class MultiTenantService : IMultiTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<MultiTenantService> _logger;
    private readonly ICacheService _cacheService;

    // Mock data - in production, this would come from a database
    private static readonly Dictionary<string, TenantInfo> _tenants = new();

    public MultiTenantService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<MultiTenantService> logger,
        ICacheService cacheService)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _cacheService = cacheService;
    }

    public string GetCurrentTenantId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id");
        return claim?.Value ?? "default";
    }

    public async Task<TenantInfo> GetTenantInfoAsync(string tenantId)
    {
        // Try cache first
        var cached = await _cacheService.GetAsync<TenantInfo>($"tenant:{tenantId}");
        if (cached != null)
            return cached;

        // In production, fetch from database
        if (_tenants.TryGetValue(tenantId, out var tenant))
        {
            await _cacheService.SetAsync($"tenant:{tenantId}", tenant, TimeSpan.FromHours(1));
            return tenant;
        }

        return new TenantInfo
        {
            TenantId = tenantId,
            Name = "Default Tenant",
            Type = "Club",
            Country = "US",
            Package = new TenantPackage
            {
                PackageName = "Scout",
                IncludedModules = new List<string> { "Basic" },
                MaxUsers = 10,
                MaxPlayers = 50,
                IncludesInjuryPrevention = false,
                IncludesTransferValuation = false,
                IncludesHighSchoolPackage = false,
                IncludesCoachingFeedback = true,
                ApiCallsPerMonth = 1000,
                RealtimeDataAccess = false,
                MonthlyPrice = 29.99m
            },
            CustomSettings = new Dictionary<string, object>(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<bool> IsTenantActiveAsync(string tenantId)
    {
        var tenant = await GetTenantInfoAsync(tenantId);
        return tenant?.IsActive ?? false;
    }

    public async Task<TenantPackage> GetTenantPackageAsync(string tenantId)
    {
        var tenant = await GetTenantInfoAsync(tenantId);
        return tenant?.Package ?? GetDefaultPackage();
    }

    public async Task<List<TenantUser>> GetTenantUsersAsync(string tenantId)
    {
        var cacheKey = $"tenant_users:{tenantId}";
        var cached = await _cacheService.GetAsync<List<TenantUser>>(cacheKey);
        if (cached != null)
            return cached;

        // In production, fetch from database
        var users = new List<TenantUser>();
        await _cacheService.SetAsync(cacheKey, users, TimeSpan.FromHours(1));
        return users;
    }

    public async Task AddTenantUserAsync(string tenantId, TenantUser user)
    {
        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;

        // In production, save to database
        _logger.LogInformation($"Added user {user.UserId} to tenant {tenantId}");

        // Invalidate cache
        await _cacheService.RemoveAsync($"tenant_users:{tenantId}");
    }

    public async Task RemoveTenantUserAsync(string tenantId, string userId)
    {
        // In production, remove from database
        _logger.LogInformation($"Removed user {userId} from tenant {tenantId}");

        // Invalidate cache
        await _cacheService.RemoveAsync($"tenant_users:{tenantId}");
    }

    private TenantPackage GetDefaultPackage()
    {
        return new TenantPackage
        {
            PackageName = "Scout",
            IncludedModules = new List<string> { "Scouting", "Analytics" },
            MaxUsers = 5,
            MaxPlayers = 200,
            IncludesInjuryPrevention = false,
            IncludesTransferValuation = false,
            IncludesHighSchoolPackage = false,
            IncludesCoachingFeedback = false,
            ApiCallsPerMonth = 10000,
            RealtimeDataAccess = false,
            MonthlyPrice = 499
        };
    }

    public static TenantPackage CreateEnterprisePackage()
    {
        return new TenantPackage
        {
            PackageName = "Enterprise",
            IncludedModules = new List<string> 
            { 
                "Scouting", 
                "Analytics", 
                "InjuryPrevention", 
                "TransferValuation", 
                "CoachingFeedback" 
            },
            MaxUsers = 100,
            MaxPlayers = 10000,
            IncludesInjuryPrevention = true,
            IncludesTransferValuation = true,
            IncludesHighSchoolPackage = true,
            IncludesCoachingFeedback = true,
            ApiCallsPerMonth = 1000000,
            RealtimeDataAccess = true,
            MonthlyPrice = 9999
        };
    }

    public static TenantPackage CreateHighSchoolPackage()
    {
        return new TenantPackage
        {
            PackageName = "HighSchool",
            IncludedModules = new List<string> 
            { 
                "Scouting", 
                "Analytics", 
                "InjuryPrevention",
                "CoachingFeedback" 
            },
            MaxUsers = 15,
            MaxPlayers = 300,
            IncludesInjuryPrevention = true,
            IncludesTransferValuation = false,
            IncludesHighSchoolPackage = true,
            IncludesCoachingFeedback = true,
            ApiCallsPerMonth = 50000,
            RealtimeDataAccess = false,
            MonthlyPrice = 1999
        };
    }
}