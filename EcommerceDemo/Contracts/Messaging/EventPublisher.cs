using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Contracts.Messaging;

/// <summary>
/// Publishes integration events to RabbitMQ with distributed tracing support.
/// Injects the W3C traceparent into message headers so consumers can link spans.
/// </summary>
public class EventPublisher : IEventPublisher
{
    private readonly IRabbitMqConnection _connection;
    private readonly ILogger<EventPublisher> _logger;
    private static readonly ActivitySource ActivitySource = new("RabbitMQ");

    public EventPublisher(IRabbitMqConnection connection, ILogger<EventPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task PublishAsync<T>(string exchange, string routingKey, T @event, CancellationToken cancellationToken = default)
        where T : class
    {
        using var activity = ActivitySource.StartActivity($"Publish {routingKey}", ActivityKind.Producer);
        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination", routingKey);
        activity?.SetTag("messaging.exchange", exchange);

        if (!_connection.IsConnected)
            await _connection.TryConnectAsync();

        await using var channel = await _connection.CreateChannelAsync();

        var json = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        // Inject W3C traceparent into message headers for distributed tracing
        var headers = new Dictionary<string, object?>();
        if (Activity.Current is not null)
        {
            var traceparent = $"00-{Activity.Current.TraceId}-{Activity.Current.SpanId}-01";
            headers["traceparent"] = Encoding.UTF8.GetBytes(traceparent);
        }
        properties.Headers = headers;

        await channel.BasicPublishAsync(exchange, routingKey, true, properties, body, cancellationToken);

        _logger.LogInformation("Published event {EventType} to {Exchange}/{RoutingKey}", typeof(T).Name, exchange, routingKey);
    }
}
