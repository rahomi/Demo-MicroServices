using Contracts.Messaging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Orders.Data;
using Orders.Domain;
using Orders.Features.Commands;
using Orders.Features.Queries;

var builder = WebApplication.CreateBuilder(args);

// --- EF Core InMemory ---
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseInMemoryDatabase("OrdersDb"));

// --- MediatR ---
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

// --- RabbitMQ messaging ---
builder.Services.AddRabbitMqMessaging(builder.Configuration);

// --- OpenAPI / Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Orders Service API",
        Version = "v1",
        Description = "Orders microservice with MediatR CQRS, EF Core InMemory, and RabbitMQ integration events."
    });
});

var app = builder.Build();

var logger = app.Logger;
app.Lifetime.ApplicationStarted.Register(() => logger.LogInformation("Orders service started."));

// --- Swagger UI (available in all environments for this demo) ---
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders Service API v1");
});

// --- Minimal API endpoints ---

app.MapPost("/api/orders", async (SubmitOrderCommand command, IMediator mediator, CancellationToken ct) =>
{
    var order = await mediator.Send(command, ct);
    return Results.Created($"/api/orders/{order.Id}", order);
})
.WithName("SubmitOrder")
.WithSummary("Submit a new order")
.WithDescription("Creates a new order, persists it, and publishes an OrderSubmitted integration event.")
.Produces<Order>(StatusCodes.Status201Created);

app.MapGet("/api/orders/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    var order = await mediator.Send(new GetOrderByIdQuery(id), ct);
    return order is not null ? Results.Ok(order) : Results.NotFound();
})
.WithName("GetOrderById")
.WithSummary("Get an order by ID")
.WithDescription("Returns a single order with its line items by unique identifier.")
.Produces<Order>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapGet("/api/orders", async (string customerId, IMediator mediator, CancellationToken ct) =>
{
    var orders = await mediator.Send(new GetOrdersByCustomerQuery(customerId), ct);
    return Results.Ok(orders);
})
.WithName("GetOrdersByCustomer")
.WithSummary("Get orders by customer")
.WithDescription("Returns all orders for a given customer, most recent first.")
.Produces<List<Order>>(StatusCodes.Status200OK);

/// <summary>
/// Cancel an order (saga compensating action).
/// </summary>
/// <param name="id">The order ID.</param>
/// <returns>The cancelled order.</returns>
/// <response code="200">Order cancelled successfully.</response>
/// <response code="404">Order not found.</response>
app.MapDelete("/api/orders/{id:guid}/cancel", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    var order = await mediator.Send(new CancelOrderCommand(id), ct);
    return order is not null ? Results.Ok(order) : Results.NotFound();
})
.WithName("CancelOrder")
.WithSummary("Cancel an order (saga compensation)")
.WithDescription("Sets the order status to 'Cancelled'. Used by the BFF saga orchestrator as a compensating action when a checkout step fails after order creation.")
.Produces<Order>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.Run();
