using System.Text.Json;
using Contracts.Events;
using Contracts.Messaging;
using Microsoft.Extensions.Logging;

namespace Notifications.Consumers;

/// <summary>
/// Consumes ProductChanged events from RabbitMQ, logs them, and stores them in the in-memory received-events store.
/// </summary>
public class ProductChangedConsumer : EventConsumer<ProductChanged>
{
    private readonly ReceivedEvents _store;
    private readonly ILogger<ProductChangedConsumer> _logger;

    public ProductChangedConsumer(
        IRabbitMqConnection connection,
        ReceivedEvents store,
        ILogger<ProductChangedConsumer> logger)
        : base(connection, logger, "notifications.product-changed", "product.changed")
    {
        _store = store;
        _logger = logger;
    }

    protected override Task HandleAsync(ProductChanged @event, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(@event);

        _store.Add(new ReceivedEvent(
            EventType: nameof(ProductChanged),
            RoutingKey: "product.changed",
            Payload: payload,
            ReceivedAt: DateTime.UtcNow));

        _logger.LogInformation(
            "Received ProductChanged: ProductId={ProductId}, Name={Name}, Price={Price}, ChangeType={ChangeType}",
            @event.ProductId, @event.Name, @event.Price, @event.ChangeType);

        return Task.CompletedTask;
    }
}
