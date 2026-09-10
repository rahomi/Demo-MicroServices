using Baskets.Data;
using Baskets.Domain;
using Contracts.Events;
using Contracts.Messaging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Baskets.Features.Commands;

/// <summary>
/// Command to add an item to a customer's basket (creates the basket if it doesn't exist).
/// </summary>
public record AddBasketItemCommand(
    string CustomerId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity) : IRequest<Basket>;

public class AddBasketItemHandler : IRequestHandler<AddBasketItemCommand, Basket>
{
    private readonly BasketDbContext _db;

    public AddBasketItemHandler(BasketDbContext db)
    {
        _db = db;
    }

    public async Task<Basket> Handle(AddBasketItemCommand request, CancellationToken cancellationToken)
    {
        var basket = await _db.Baskets
            .FirstOrDefaultAsync(b => b.CustomerId == request.CustomerId, cancellationToken);

        if (basket is null)
        {
            basket = new Basket { Id = Guid.NewGuid(), CustomerId = request.CustomerId };
            _db.Baskets.Add(basket);
            await _db.SaveChangesAsync(cancellationToken);
        }

        // If the product is already in the basket, increase the quantity; otherwise add a new line.
        var existingItem = await _db.BasketItems
            .FirstOrDefaultAsync(i => i.BasketId == basket.Id && i.ProductId == request.ProductId, cancellationToken);

        if (existingItem is not null)
        {
            existingItem.Quantity += request.Quantity;
            existingItem.UnitPrice = request.UnitPrice;
            existingItem.ProductName = request.ProductName;
        }
        else
        {
            _db.BasketItems.Add(new BasketItem
            {
                Id = Guid.NewGuid(),
                BasketId = basket.Id,
                ProductId = request.ProductId,
                ProductName = request.ProductName,
                UnitPrice = request.UnitPrice,
                Quantity = request.Quantity
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        // Return the basket with its items loaded
        return (await _db.Baskets
            .Include(b => b.Items)
            .FirstAsync(b => b.Id == basket.Id, cancellationToken));
    }
}

/// <summary>
/// Command to remove an item from a customer's basket by product ID.
/// </summary>
public record RemoveBasketItemCommand(string CustomerId, Guid ProductId) : IRequest<Basket?>;

public class RemoveBasketItemHandler : IRequestHandler<RemoveBasketItemCommand, Basket?>
{
    private readonly BasketDbContext _db;

    public RemoveBasketItemHandler(BasketDbContext db)
    {
        _db = db;
    }

    public async Task<Basket?> Handle(RemoveBasketItemCommand request, CancellationToken cancellationToken)
    {
        var basket = await _db.Baskets
            .FirstOrDefaultAsync(b => b.CustomerId == request.CustomerId, cancellationToken);

        if (basket is null)
            return null;

        var item = await _db.BasketItems
            .FirstOrDefaultAsync(i => i.BasketId == basket.Id && i.ProductId == request.ProductId, cancellationToken);

        if (item is not null)
        {
            _db.BasketItems.Remove(item);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return await _db.Baskets
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == basket.Id, cancellationToken);
    }
}

/// <summary>
/// Command to check out a customer's basket: clears the basket items and publishes a BasketCheckedOut event.
/// </summary>
public record CheckoutBasketCommand(string CustomerId) : IRequest<BasketCheckedOut?>;

public class CheckoutBasketHandler : IRequestHandler<CheckoutBasketCommand, BasketCheckedOut?>
{
    private readonly BasketDbContext _db;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<CheckoutBasketHandler> _logger;

    public CheckoutBasketHandler(BasketDbContext db, IEventPublisher publisher, ILogger<CheckoutBasketHandler> logger)
    {
        _db = db;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<BasketCheckedOut?> Handle(CheckoutBasketCommand request, CancellationToken cancellationToken)
    {
        var basket = await _db.Baskets
            .FirstOrDefaultAsync(b => b.CustomerId == request.CustomerId, cancellationToken);

        if (basket is null)
            return null;

        var items = await _db.BasketItems
            .Where(i => i.BasketId == basket.Id)
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
            return null;

        var @event = new BasketCheckedOut(
            basket.CustomerId,
            items.Select(i => new BasketItemDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList(),
            DateTime.UtcNow);

        // Clear the basket after checkout
        _db.BasketItems.RemoveRange(items);
        await _db.SaveChangesAsync(cancellationToken);

        await TryPublishAsync(@event, cancellationToken);

        return @event;
    }

    private async Task TryPublishAsync(BasketCheckedOut @event, CancellationToken ct)
    {
        try
        {
            await _publisher.PublishAsync("amq.topic", "basket.checkedout", @event, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish BasketCheckedOut event for customer {CustomerId}. Event will not be delivered.", @event.CustomerId);
        }
    }
}
