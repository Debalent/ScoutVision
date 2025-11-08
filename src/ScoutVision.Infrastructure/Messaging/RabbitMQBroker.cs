using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ScoutVision.Infrastructure.Messaging;

public class RabbitMQBroker : IMessageBroker, IDisposable
{
    private readonly IConnectionFactory _connectionFactory;
    private IConnection _connection;
    private IModel _channel;
    private readonly ILogger<RabbitMQBroker> _logger;

    public RabbitMQBroker(IConnectionFactory connectionFactory, ILogger<RabbitMQBroker> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        Connect();
    }

    private void Connect()
    {
        try
        {
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            
            _channel.BasicQos(0, 100, false);
            
            _logger.LogInformation("Connected to RabbitMQ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ");
            throw;
        }
    }

    public async Task PublishAsync<T>(string queue, T message) where T : class
    {
        await PublishAsync(queue, JsonSerializer.Serialize(message));
    }

    public async Task PublishAsync(string queue, string message)
    {
        try
        {
            if (_channel == null || !_channel.IsOpen)
            {
                Connect();
            }

            _channel.QueueDeclare(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: "",
                routingKey: queue,
                basicProperties: properties,
                body: body
            );

            _logger.LogInformation($"Message published to queue: {queue}");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to publish message to queue: {queue}");
            throw;
        }
    }

    public async Task SubscribeAsync<T>(string queue, Func<T, Task> handler) where T : class
    {
        try
        {
            _channel.QueueDeclare(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            var consumer = new AsyncEventingBasicConsumer(_channel);
            
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var data = JsonSerializer.Deserialize<T>(message);

                    await handler(data);
                    
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from queue");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(
                queue: queue,
                autoAck: false,
                consumerTag: $"consumer-{Guid.NewGuid()}",
                consumer: consumer
            );

            _logger.LogInformation($"Subscription established for queue: {queue}");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to subscribe to queue: {queue}");
            throw;
        }
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            return _channel != null && _channel.IsOpen && _connection != null && _connection.IsOpen;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ connection");
        }
    }
}