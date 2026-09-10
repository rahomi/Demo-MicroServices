using Baskets.Data;
using Contracts.Events;
using Contracts.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Baskets.Features.Consumers;

/// <summary>
/// Consumes ProductChanged events from RabbitMQ and updates basket item names/prices
/// to keep them in sync with the Products service.
/// </summary>
public class ProductChangedConsumer : EventConsumer<ProductChanged>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProductChangedConsumer> _logger;

    public ProductChangedConsumer(
        IRabbitMqConnection connection,
        IServiceProvider serviceProvider,
        ILogger<ProductChangedConsumer> logger)
        : base(connection, logger, "baskets.product-changed", "product.changed")
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task HandleAsync(ProductChanged @event, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BasketDbContext>();

        var items = await db.BasketItems
            .Where(i => i.ProductId == @event.ProductId)
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
            return;

        if (@event.ChangeType == "deleted")
        {
            // Remove basket items for deleted products
            db.BasketItems.RemoveRange(items);
        }
        else
        {
            // Update name and price for created/updated products
            foreach (var item in items)
            {
                item.ProductName = @event.Name;
                item.UnitPrice = @event.Price;
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Processed ProductChanged ({ChangeType}) for product {ProductId}: {ItemCount} basket items updated.",
            @event.ChangeType, @event.ProductId, items.Count);
    }
}
