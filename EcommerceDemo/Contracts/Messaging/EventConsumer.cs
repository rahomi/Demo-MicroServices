using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Contracts.Messaging;

/// <summary>
/// Base class for RabbitMQ event consumers implemented as a BackgroundService.
/// Extracts the W3C traceparent from message headers and creates a linked activity
/// so distributed traces span the async messaging boundary.
/// </summary>
public abstract class EventConsumer<T> : BackgroundService where T : class
{
    private readonly IRabbitMqConnection _connection;
    private readonly ILogger _logger;
    private readonly string _queueName;
    private readonly string _routingKey;
    private static readonly ActivitySource ActivitySource = new("RabbitMQ");

    protected EventConsumer(IRabbitMqConnection connection, ILogger logger, string queueName, string routingKey)
    {
        _connection = connection;
        _logger = logger;
        _queueName = queueName;
        _routingKey = routingKey;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!await _connection.TryConnectAsync())
        {
            _logger.LogWarning("Could not connect to RabbitMQ for consumer {QueueName}. Retrying in 5s...", _queueName);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            return;
        }

        await using var channel = await _connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(_queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await channel.QueueBindAsync(_queueName, "amq.topic", _routingKey, cancellationToken: stoppingToken);
        await channel.BasicQosAsync(0, 1, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var @event = JsonSerializer.Deserialize<T>(json);

                // Extract traceparent from headers and create a linked activity
                Activity? linkedActivity = null;
                if (ea.BasicProperties.Headers is not null &&
                    ea.BasicProperties.Headers.TryGetValue("traceparent", out var traceparentObj))
                {
                    var traceparent = Encoding.UTF8.GetString((byte[])traceparentObj!);
                    var parts = traceparent.Split('-');
                    if (parts.Length >= 3)
                    {
                        var parentId = parts[2];
                        linkedActivity = ActivitySource.StartActivity($"Consume {_routingKey}", ActivityKind.Consumer, parentId);
                    }
                }

                using (linkedActivity)
                {
                    linkedActivity?.SetTag("messaging.system", "rabbitmq");
                    linkedActivity?.SetTag("messaging.destination", _queueName);

                    await HandleAsync(@event!, stoppingToken);
                }

                await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming message from {QueueName}", _queueName);
                await channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(_queueName, autoAck: false, consumer, stoppingToken);

        _logger.LogInformation("Consumer for {QueueName} ({RoutingKey}) started.", _queueName, _routingKey);

        // Keep the service running until cancelled
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Expected on shutdown
        }
    }

    /// <summary>
    /// Override this method to handle the deserialized event.
    /// </summary>
    protected abstract Task HandleAsync(T @event, CancellationToken cancellationToken);
}
