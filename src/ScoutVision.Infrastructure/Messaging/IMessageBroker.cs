namespace ScoutVision.Infrastructure.Messaging;

public interface IMessageBroker
{
    Task PublishAsync<T>(string queue, T message) where T : class;
    Task PublishAsync(string queue, string message);
    Task SubscribeAsync<T>(string queue, Func<T, Task> handler) where T : class;
    Task<bool> IsHealthyAsync();
}

public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public object Data { get; set; }
    public string MessageType { get; set; }
}

// Event messages for different modules
public class PlayerAnalyticsEvent : Message
{
    public string PlayerId { get; set; }
    public Dictionary<string, object> Metrics { get; set; }
}

public class InjuryAlertEvent : Message
{
    public string PlayerId { get; set; }
    public string ClubId { get; set; }
    public int RiskScore { get; set; }
    public string InjuryType { get; set; }
    public string Recommendation { get; set; }
}

public class TransferValuationEvent : Message
{
    public string PlayerId { get; set; }
    public decimal MarketValue { get; set; }
    public string BuySellHold { get; set; }
    public List<TransferComparable> ComparablePlayers { get; set; }
}

public class TransferComparable
{
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public decimal Value { get; set; }
    public double Similarity { get; set; }
}

public class DataIntegrationEvent : Message
{
    public string Source { get; set; }
    public string DataType { get; set; }
    public int RecordsProcessed { get; set; }
}