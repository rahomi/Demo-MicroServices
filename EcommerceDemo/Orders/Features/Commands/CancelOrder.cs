using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Orders.Data;
using Orders.Domain;

namespace Orders.Features.Commands;

/// <summary>
/// Command to cancel an order (saga compensating action).
/// Sets the order status to "Cancelled".
/// </summary>
public record CancelOrderCommand(Guid OrderId) : IRequest<Order?>;

public class CancelOrderHandler : IRequestHandler<CancelOrderCommand, Order?>
{
    private readonly OrderDbContext _db;
    private readonly ILogger<CancelOrderHandler> _logger;

    public CancelOrderHandler(OrderDbContext db, ILogger<CancelOrderHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Order?> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling order {OrderId} (saga compensation).", request.OrderId);

        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            return null;

        order.Status = "Cancelled";
        await _db.SaveChangesAsync(cancellationToken);

        return order;
    }
}
