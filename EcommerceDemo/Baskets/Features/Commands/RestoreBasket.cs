using Baskets.Data;
using Baskets.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Baskets.Features.Commands;

/// <summary>
/// Request body for restoring basket items (saga compensation).
/// </summary>
public record RestoreBasketItemRequest(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);

/// <summary>
/// Command to restore basket items from a snapshot (saga compensating action).
/// Re-adds the given items to the customer's basket, creating the basket if it doesn't exist.
/// </summary>
public record RestoreBasketCommand(
    string CustomerId,
    List<RestoreBasketItemRequest> Items) : IRequest<Basket>;

public class RestoreBasketHandler : IRequestHandler<RestoreBasketCommand, Basket>
{
    private readonly BasketDbContext _db;
    private readonly ILogger<RestoreBasketHandler> _logger;

    public RestoreBasketHandler(BasketDbContext db, ILogger<RestoreBasketHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Basket> Handle(RestoreBasketCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Restoring basket for customer {CustomerId} with {ItemCount} items (saga compensation).",
            request.CustomerId, request.Items.Count);

        var basket = await _db.Baskets
            .FirstOrDefaultAsync(b => b.CustomerId == request.CustomerId, cancellationToken);

        if (basket is null)
        {
            basket = new Basket { Id = Guid.NewGuid(), CustomerId = request.CustomerId };
            _db.Baskets.Add(basket);
            await _db.SaveChangesAsync(cancellationToken);
        }

        foreach (var item in request.Items)
        {
            var existingItem = await _db.BasketItems
                .FirstOrDefaultAsync(i => i.BasketId == basket.Id && i.ProductId == item.ProductId, cancellationToken);

            if (existingItem is not null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                _db.BasketItems.Add(new BasketItem
                {
                    Id = Guid.NewGuid(),
                    BasketId = basket.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity
                });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        return await _db.Baskets
            .Include(b => b.Items)
            .FirstAsync(b => b.Id == basket.Id, cancellationToken);
    }
}
