using Contracts.Events;
using Contracts.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;
using Orders.Data;
using Orders.Domain;

namespace Orders.Features.Commands;

/// <summary>
/// Command to submit a new order. Creates the order, persists it, and publishes an OrderSubmitted integration event.
/// </summary>
public record SubmitOrderCommand(
    string CustomerId,
    List<SubmitOrderItem> Items) : IRequest<Order>;

/// <summary>
/// A line item in a submit-order request.
/// </summary>
public record SubmitOrderItem(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);

public class SubmitOrderHandler : IRequestHandler<SubmitOrderCommand, Order>
{
    private readonly OrderDbContext _db;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<SubmitOrderHandler> _logger;

    public SubmitOrderHandler(OrderDbContext db, IEventPublisher publisher, ILogger<SubmitOrderHandler> logger)
    {
        _db = db;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Order> Handle(SubmitOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Status = "Submitted",
            CreatedAt = DateTime.UtcNow,
            Items = request.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList()
        };

        order.Total = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);

        var @event = new OrderSubmitted(
            order.Id,
            order.CustomerId,
            order.Total,
            order.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList(),
            order.CreatedAt);

        await TryPublishAsync(@event, cancellationToken);

        return order;
    }

    private async Task TryPublishAsync(OrderSubmitted @event, CancellationToken ct)
    {
        try
        {
            await _publisher.PublishAsync("amq.topic", "order.submitted", @event, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish OrderSubmitted event for order {OrderId}. Event will not be delivered.", @event.OrderId);
        }
    }
}
