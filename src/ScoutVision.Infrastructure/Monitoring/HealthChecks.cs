using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using RabbitMQ.Client;
using ScoutVision.Infrastructure.Data;
using ScoutVision.Infrastructure.Messaging;

namespace ScoutVision.Infrastructure.Monitoring;

/// <summary>
/// Database health check for SQL Server
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ScoutVisionDbContext _context;

    public DatabaseHealthCheck(ScoutVisionDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            if (canConnect)
            {
                return HealthCheckResult.Healthy("SQL Server database is healthy");
            }
            return HealthCheckResult.Unhealthy("Cannot connect to SQL Server database");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"SQL Server database check failed: {ex.Message}");
        }
    }
}

/// <summary>
/// Database health check for PostgreSQL TimescaleDB
/// </summary>
public class TimeSeriesHealthCheck : IHealthCheck
{
    private readonly TimeSeriesContext _context;

    public TimeSeriesHealthCheck(TimeSeriesContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            if (canConnect)
            {
                return HealthCheckResult.Healthy("TimescaleDB is healthy");
            }
            return HealthCheckResult.Unhealthy("Cannot connect to TimescaleDB");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"TimescaleDB check failed: {ex.Message}");
        }
    }
}

/// <summary>
/// Redis health check
/// </summary>
public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _connection;

    public RedisHealthCheck(IConnectionMultiplexer connection)
    {
        _connection = connection;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _connection.GetDatabase();
            await db.PingAsync();
            return HealthCheckResult.Healthy("Redis is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Redis check failed: {ex.Message}");
        }
    }
}

/// <summary>
/// RabbitMQ health check
/// </summary>
public class RabbitMQHealthCheck : IHealthCheck
{
    private readonly IMessageBroker _messageBroker;

    public RabbitMQHealthCheck(IMessageBroker messageBroker)
    {
        _messageBroker = messageBroker;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var isHealthy = await _messageBroker.IsHealthyAsync();
            if (isHealthy)
            {
                return HealthCheckResult.Healthy("RabbitMQ is healthy");
            }
            return HealthCheckResult.Unhealthy("RabbitMQ connection is not healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"RabbitMQ check failed: {ex.Message}");
        }
    }
}

/// <summary>
/// Response writer for detailed health check information
/// </summary>
public class HealthCheckResponseWriter
{
    public static async Task WriteResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            })
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Service for monitoring system metrics
/// </summary>
public interface ISystemMetricsService
{
    Task<SystemMetrics> GetMetricsAsync();
}

public class SystemMetricsService : ISystemMetricsService
{
    private readonly ILogger<SystemMetricsService> _logger;

    public SystemMetricsService(ILogger<SystemMetricsService> logger)
    {
        _logger = logger;
    }

    public async Task<SystemMetrics> GetMetricsAsync()
    {
        var process = System.Diagnostics.Process.GetCurrentProcess();

        return new SystemMetrics
        {
            Timestamp = DateTime.UtcNow,
            MemoryUsageMB = process.WorkingSet64 / (1024 * 1024),
            CpuUsagePercent = GetCpuUsage(process),
            ThreadCount = process.Threads.Count,
            HandleCount = process.HandleCount,
            Uptime = DateTime.UtcNow - process.StartTime
        };
    }

    private double GetCpuUsage(System.Diagnostics.Process process)
    {
        try
        {
            var totalMilliseconds = (DateTime.UtcNow - process.StartTime).TotalMilliseconds;
            var totalMicroseconds = process.TotalProcessorTime.TotalMilliseconds;
            var cpuUsageTotal = totalMicroseconds / totalMilliseconds / Environment.ProcessorCount;
            return cpuUsageTotal * 100;
        }
        catch
        {
            return 0;
        }
    }
}

public class SystemMetrics
{
    public DateTime Timestamp { get; set; }
    public long MemoryUsageMB { get; set; }
    public double CpuUsagePercent { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    public TimeSpan Uptime { get; set; }
}