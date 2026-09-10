using Contracts.Events;
using Contracts.Messaging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Products.Data;
using Products.Domain;

namespace Products.Features.Commands;

/// <summary>
/// Command to create a new product.
/// </summary>
public record CreateProductCommand(string Name, decimal Price, string Category) : IRequest<Product>;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Product>
{
    private readonly ProductDbContext _db;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<CreateProductHandler> _logger;

    public CreateProductHandler(ProductDbContext db, IEventPublisher publisher, ILogger<CreateProductHandler> logger)
    {
        _db = db;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Product> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Price = request.Price,
            Category = request.Category
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync(cancellationToken);

        await TryPublishAsync(new ProductChanged(product.Id, product.Name, product.Price, "created"), cancellationToken);

        return product;
    }

    private async Task TryPublishAsync(ProductChanged @event, CancellationToken ct)
    {
        try
        {
            await _publisher.PublishAsync("amq.topic", "product.changed", @event, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish ProductChanged event for product {ProductId}. Event will not be delivered.", @event.ProductId);
        }
    }
}

/// <summary>
/// Command to update an existing product.
/// </summary>
public record UpdateProductCommand(Guid Id, string Name, decimal Price, string Category) : IRequest<Product?>;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Product?>
{
    private readonly ProductDbContext _db;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<UpdateProductHandler> _logger;

    public UpdateProductHandler(ProductDbContext db, IEventPublisher publisher, ILogger<UpdateProductHandler> logger)
    {
        _db = db;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Product?> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _db.Products.FindAsync([request.Id], cancellationToken);
        if (product is null)
            return null;

        product.Name = request.Name;
        product.Price = request.Price;
        product.Category = request.Category;

        await _db.SaveChangesAsync(cancellationToken);

        await TryPublishAsync(new ProductChanged(product.Id, product.Name, product.Price, "updated"), cancellationToken);

        return product;
    }

    private async Task TryPublishAsync(ProductChanged @event, CancellationToken ct)
    {
        try
        {
            await _publisher.PublishAsync("amq.topic", "product.changed", @event, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish ProductChanged event for product {ProductId}. Event will not be delivered.", @event.ProductId);
        }
    }
}

/// <summary>
/// Command to delete a product.
/// </summary>
public record DeleteProductCommand(Guid Id) : IRequest<bool>;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly ProductDbContext _db;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<DeleteProductHandler> _logger;

    public DeleteProductHandler(ProductDbContext db, IEventPublisher publisher, ILogger<DeleteProductHandler> logger)
    {
        _db = db;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _db.Products.FindAsync([request.Id], cancellationToken);
        if (product is null)
            return false;

        _db.Products.Remove(product);
        await _db.SaveChangesAsync(cancellationToken);

        await TryPublishAsync(new ProductChanged(product.Id, product.Name, product.Price, "deleted"), cancellationToken);

        return true;
    }

    private async Task TryPublishAsync(ProductChanged @event, CancellationToken ct)
    {
        try
        {
            await _publisher.PublishAsync("amq.topic", "product.changed", @event, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish ProductChanged event for product {ProductId}. Event will not be delivered.", @event.ProductId);
        }
    }
}
