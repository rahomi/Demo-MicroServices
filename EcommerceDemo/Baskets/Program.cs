using Baskets.Data;
using Baskets.Domain;
using Baskets.Features.Commands;
using Baskets.Features.Consumers;
using Baskets.Features.Queries;
using Contracts.Events;
using Contracts.Messaging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// --- OpenTelemetry distributed tracing ---
builder.Services.AddOpenTelemetryTracing(builder.Configuration);

// --- EF Core InMemory ---
builder.Services.AddDbContext<BasketDbContext>(options =>
    options.UseInMemoryDatabase("BasketsDb"));

// --- MediatR ---
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

// --- RabbitMQ messaging ---
builder.Services.AddRabbitMqMessaging(builder.Configuration);

// --- ProductChanged consumer (BackgroundService) ---
builder.Services.AddHostedService<ProductChangedConsumer>();

// --- OpenAPI / Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Baskets Service API",
        Version = "v1",
        Description = "Baskets microservice with MediatR CQRS, EF Core InMemory, RabbitMQ integration events, and ProductChanged event consumption."
    });
});

var app = builder.Build();

var logger = app.Logger;
app.Lifetime.ApplicationStarted.Register(() => logger.LogInformation("Baskets service started."));

// --- Swagger UI (available in all environments for this demo) ---
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Baskets Service API v1");
});

// --- Minimal API endpoints ---

/// <summary>
/// Get a customer's basket.
/// </summary>
/// <param name="customerId">The customer ID.</param>
/// <returns>The customer's basket with all items.</returns>
/// <response code="200">Returns the basket (empty if none exists).</response>
app.MapGet("/api/baskets/{customerId}", async (string customerId, IMediator mediator, CancellationToken ct) =>
{
    var basket = await mediator.Send(new GetBasketQuery(customerId), ct);
    return Results.Ok(basket ?? new Basket { CustomerId = customerId, Items = [] });
})
.WithName("GetBasket")
.WithSummary("Get a customer's basket")
.WithDescription("Returns the basket for the given customer ID. Returns an empty basket if none exists.")
.Produces<Basket>(StatusCodes.Status200OK);

/// <summary>
/// Add an item to a customer's basket.
/// </summary>
/// <param name="customerId">The customer ID.</param>
/// <param name="item">The item to add.</param>
/// <returns>The updated basket.</returns>
/// <response code="200">Item added successfully.</response>
app.MapPost("/api/baskets/{customerId}/items", async (string customerId, AddBasketItemRequest item, IMediator mediator, CancellationToken ct) =>
{
    var basket = await mediator.Send(new AddBasketItemCommand(
        customerId,
        item.ProductId,
        item.ProductName,
        item.UnitPrice,
        item.Quantity), ct);
    return Results.Ok(basket);
})
.WithName("AddBasketItem")
.WithSummary("Add an item to a customer's basket")
.WithDescription("Adds an item to the basket, creating the basket if it doesn't exist. If the product is already in the basket, the quantity is increased.")
.Produces<Basket>(StatusCodes.Status200OK);

/// <summary>
/// Remove an item from a customer's basket.
/// </summary>
/// <param name="customerId">The customer ID.</param>
/// <param name="productId">The product ID to remove.</param>
/// <returns>The updated basket.</returns>
/// <response code="200">Item removed successfully.</response>
/// <response code="404">Basket not found.</response>
app.MapDelete("/api/baskets/{customerId}/items/{productId:guid}", async (string customerId, Guid productId, IMediator mediator, CancellationToken ct) =>
{
    var basket = await mediator.Send(new RemoveBasketItemCommand(customerId, productId), ct);
    return basket is not null ? Results.Ok(basket) : Results.NotFound();
})
.WithName("RemoveBasketItem")
.WithSummary("Remove an item from a customer's basket")
.WithDescription("Removes the item with the given product ID from the basket.")
.Produces<Basket>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

/// <summary>
/// Check out a customer's basket.
/// </summary>
/// <param name="customerId">The customer ID.</param>
/// <returns>The checkout event with basket contents.</returns>
/// <response code="200">Checkout successful.</response>
/// <response code="404">Basket is empty or not found.</response>
app.MapPost("/api/baskets/{customerId}/checkout", async (string customerId, IMediator mediator, CancellationToken ct) =>
{
    var checkout = await mediator.Send(new CheckoutBasketCommand(customerId), ct);
    return checkout is not null ? Results.Ok(checkout) : Results.NotFound("Basket is empty or not found.");
})
.WithName("CheckoutBasket")
.WithSummary("Check out a customer's basket")
.WithDescription("Clears the basket and publishes a BasketCheckedOut integration event to RabbitMQ.")
.Produces<BasketCheckedOut>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

/// <summary>
/// Restore basket items from a snapshot (saga compensating action).
/// </summary>
/// <param name="customerId">The customer ID.</param>
/// <param name="items">The items to restore.</param>
/// <returns>The restored basket.</returns>
/// <response code="200">Items restored successfully.</response>
app.MapPost("/api/baskets/{customerId}/restore", async (string customerId, List<RestoreBasketItemRequest> items, IMediator mediator, CancellationToken ct) =>
{
    var basket = await mediator.Send(new RestoreBasketCommand(customerId, items), ct);
    return Results.Ok(basket);
})
.WithName("RestoreBasket")
.WithSummary("Restore basket items (saga compensation)")
.WithDescription("Re-adds items to the basket from a snapshot. Used by the BFF saga orchestrator as a compensating action when order creation fails after basket checkout.")
.Produces<Basket>(StatusCodes.Status200OK);

app.Run();

/// <summary>
/// Request body for adding an item to a basket.
/// </summary>
public record AddBasketItemRequest(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
