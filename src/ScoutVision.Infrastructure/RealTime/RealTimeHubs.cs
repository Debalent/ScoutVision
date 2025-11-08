using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ScoutVision.Infrastructure.RealTime;

/// <summary>
/// Real-time hub for player analytics with sub-second latency updates
/// </summary>
[Authorize]
public class PlayerAnalyticsHub : Hub
{
    private readonly ILogger<PlayerAnalyticsHub> _logger;
    private readonly IHubContext<PlayerAnalyticsHub> _hubContext;

    public PlayerAnalyticsHub(ILogger<PlayerAnalyticsHub> logger, IHubContext<PlayerAnalyticsHub> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToPlayer(string playerId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"player:{playerId}");
        _logger.LogInformation($"Client {Context.ConnectionId} subscribed to player {playerId}");
    }

    public async Task UnsubscribeFromPlayer(string playerId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"player:{playerId}");
    }

    public async Task SubscribeToClub(string clubId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"club:{clubId}");
        _logger.LogInformation($"Client {Context.ConnectionId} subscribed to club {clubId}");
    }

    public async Task SendPlayerMetrics(string playerId, object metrics)
    {
        var stopwatch = Stopwatch.StartNew();
        await _hubContext.Clients.Group($"player:{playerId}").SendAsync("ReceivePlayerMetrics", playerId, metrics);
        stopwatch.Stop();
        _logger.LogInformation($"Player metrics sent in {stopwatch.ElapsedMilliseconds}ms");
    }

    public async Task SendClubAnalytics(string clubId, object analytics)
    {
        await _hubContext.Clients.Group($"club:{clubId}").SendAsync("ReceiveClubAnalytics", clubId, analytics);
    }
}

/// <summary>
/// Real-time hub for injury alerts and prevention
/// </summary>
[Authorize]
public class InjuryAlertHub : Hub
{
    private readonly ILogger<InjuryAlertHub> _logger;
    private readonly IHubContext<InjuryAlertHub> _hubContext;

    public InjuryAlertHub(ILogger<InjuryAlertHub> logger, IHubContext<InjuryAlertHub> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task SubscribeToClubAlerts(string clubId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"injury_alerts:{clubId}");
        _logger.LogInformation($"Subscribed to injury alerts for club {clubId}");
    }

    public async Task SendInjuryAlert(string clubId, object alertData)
    {
        var stopwatch = Stopwatch.StartNew();
        await _hubContext.Clients.Group($"injury_alerts:{clubId}").SendAsync("ReceiveInjuryAlert", alertData);
        stopwatch.Stop();
        _logger.LogInformation($"Injury alert sent in {stopwatch.ElapsedMilliseconds}ms");
    }

    public async Task SendRiskUpdate(string playerId, int riskScore)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveRiskUpdate", playerId, riskScore);
    }
}



/// <summary>
/// Real-time hub for transfer market valuations
/// </summary>
[Authorize]
public class TransferValueHub : Hub
{
    private readonly ILogger<TransferValueHub> _logger;
    private readonly IHubContext<TransferValueHub> _hubContext;

    public TransferValueHub(ILogger<TransferValueHub> logger, IHubContext<TransferValueHub> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task SubscribeToPlayer(string playerId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"transfer:{playerId}");
    }

    public async Task SendTransferValuation(string playerId, object valuation)
    {
        await _hubContext.Clients.Group($"transfer:{playerId}").SendAsync("ReceiveTransferValuation", valuation);
    }

    public async Task SendComparableAnalysis(string playerId, object comparables)
    {
        await _hubContext.Clients.Group($"transfer:{playerId}").SendAsync("ReceiveComparableAnalysis", comparables);
    }

    public async Task SendMarketTrend(object trendData)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveMarketTrend", trendData);
    }
}

/// <summary>
/// Service for broadcasting real-time data to connected clients
/// </summary>
public interface IRealTimeBroadcaster
{
    Task BroadcastPlayerMetrics(string playerId, object metrics);
    Task BroadcastInjuryAlert(string clubId, object alert);
    Task BroadcastTransferValuation(string playerId, object valuation);
}

public class RealTimeBroadcaster : IRealTimeBroadcaster
{
    private readonly IHubContext<PlayerAnalyticsHub> _playerAnalyticsHub;
    private readonly IHubContext<InjuryAlertHub> _injuryAlertHub;
    private readonly IHubContext<TransferValueHub> _transferHub;
    private readonly ILogger<RealTimeBroadcaster> _logger;

    public RealTimeBroadcaster(
        IHubContext<PlayerAnalyticsHub> playerAnalyticsHub,
        IHubContext<InjuryAlertHub> injuryAlertHub,
        IHubContext<TransferValueHub> transferHub,
        ILogger<RealTimeBroadcaster> logger)
    {
        _playerAnalyticsHub = playerAnalyticsHub;
        _injuryAlertHub = injuryAlertHub;
        _transferHub = transferHub;
        _logger = logger;
    }

    public async Task BroadcastPlayerMetrics(string playerId, object metrics)
    {
        try
        {
            await _playerAnalyticsHub.Clients.Group($"player:{playerId}").SendAsync("ReceivePlayerMetrics", metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error broadcasting player metrics for {playerId}");
        }
    }

    public async Task BroadcastInjuryAlert(string clubId, object alert)
    {
        try
        {
            await _injuryAlertHub.Clients.Group($"injury_alerts:{clubId}").SendAsync("ReceiveInjuryAlert", alert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error broadcasting injury alert for club {clubId}");
        }
    }

    public async Task BroadcastTransferValuation(string playerId, object valuation)
    {
        try
        {
            await _transferHub.Clients.Group($"transfer:{playerId}").SendAsync("ReceiveTransferValuation", valuation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error broadcasting transfer valuation for player {playerId}");
        }
    }
}