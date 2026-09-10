using System.Text.Json;
using Contracts.Events;
using Contracts.Messaging;
using Microsoft.Extensions.Logging;

namespace Notifications.Consumers;

/// <summary>
/// Consumes OrderSubmitted events from RabbitMQ, logs them, and stores them in the in-memory received-events store.
/// </summary>
public class OrderSubmittedConsumer : EventConsumer<OrderSubmitted>
{
    private readonly ReceivedEvents _store;
    private readonly ILogger<OrderSubmittedConsumer> _logger;

    public OrderSubmittedConsumer(
        IRabbitMqConnection connection,
        ReceivedEvents store,
        ILogger<OrderSubmittedConsumer> logger)
        : base(connection, logger, "notifications.order-submitted", "order.submitted")
    {
        _store = store;
        _logger = logger;
    }

    protected override Task HandleAsync(OrderSubmitted @event, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(@event);

        _store.Add(new ReceivedEvent(
            EventType: nameof(OrderSubmitted),
            RoutingKey: "order.submitted",
            Payload: payload,
            ReceivedAt: DateTime.UtcNow));

        _logger.LogInformation(
            "Received OrderSubmitted: OrderId={OrderId}, CustomerId={CustomerId}, Total={Total}, Items={ItemCount}",
            @event.OrderId, @event.CustomerId, @event.Total, @event.Items.Count);

        return Task.CompletedTask;
    }
}
