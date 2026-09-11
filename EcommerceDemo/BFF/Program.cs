using System.Net;
using BFF.Clients;
using BFF.Saga;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// --- Refit clients (downstream service URLs configurable via environment/config) ---
var productsUrl = builder.Configuration["Downstream:ProductsUrl"] ?? "http://localhost:5024";
var basketsUrl = builder.Configuration["Downstream:BasketsUrl"] ?? "http://localhost:5201";
var ordersUrl = builder.Configuration["Downstream:OrdersUrl"] ?? "http://localhost:5202";
var identityUrl = builder.Configuration["Downstream:IdentityUrl"] ?? "http://localhost:5003";

builder.Services.AddRefitClient<IProductsClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(productsUrl));
builder.Services.AddRefitClient<IBasketsClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(basketsUrl));
builder.Services.AddRefitClient<IOrdersClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(ordersUrl));
builder.Services.AddRefitClient<IIdentityClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(identityUrl));

// --- Saga state persistence (EF Core InMemory) ---
builder.Services.AddDbContext<SagaDbContext>(options =>
    options.UseInMemoryDatabase("SagaDb"));

// --- Saga orchestrator ---
builder.Services.AddScoped<CheckoutSagaOrchestrator>();

// --- OpenAPI / Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BFF Service API",
        Version = "v1",
        Description = "Backend for Frontend — routes to Products, Baskets, Orders, and Identity services. Includes checkout orchestration."
    });
});

var app = builder.Build();

var logger = app.Logger;
app.Lifetime.ApplicationStarted.Register(() => logger.LogInformation("BFF service started."));

// --- Swagger UI ---
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "BFF Service API v1");
});

// Helper: map downstream errors to appropriate HTTP responses
static IResult HandleDownstreamError(Exception ex, string serviceName, ILogger logger)
{
    if (ex is Refit.ApiException apiEx)
    {
        return Results.Problem(
            title: $"{serviceName} returned an error",
            statusCode: (int)apiEx.StatusCode,
            detail: apiEx.Content);
    }

    logger.LogWarning(ex, "{ServiceName} is unreachable.", serviceName);
    return Results.Problem(
        title: $"{serviceName} is unreachable",
        statusCode: StatusCodes.Status503ServiceUnavailable,
        detail: $"Could not connect to {serviceName}. It may not be running.");
}

// ==================== Identity routes ====================

app.MapGet("/api/identity/customer", async (IIdentityClient client) =>
{
    try
    {
        var customer = await client.GetCustomerAsync();
        return Results.Ok(customer);
    }
    catch (Exception ex) { return HandleDownstreamError(ex, "Identity service", logger); }
})
.WithName("GetCustomer")
.WithSummary("Get the fake customer")
.WithDescription("Proxies to the Identity service.")
.Produces<CustomerDto>(StatusCodes.Status200OK);

// ==================== Products routes ====================

app.MapGet("/api/products", async (IProductsClient client) =>
{
    try
    {
        var products = await client.GetProductsAsync();
        return Results.Ok(products);
    }
    catch (Exception ex) { return HandleDownstreamError(ex, "Products service", logger); }
})
.WithName("GetProducts")
.WithSummary("Get all products")
.WithDescription("Proxies to the Products service.")
.Produces<List<ProductDto>>(StatusCodes.Status200OK);

app.MapGet("/api/products/{id:guid}", async (Guid id, IProductsClient client) =>
{
    try
    {
        var product = await client.GetProductByIdAsync(id);
        return Results.Ok(product);
    }
    catch (Refit.ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
    {
        return Results.NotFound();
    }
    catch (Exception ex) { return HandleDownstreamError(ex, "Products service", logger); }
})
.WithName("GetProductById")
.WithSummary("Get a product by ID")
.WithDescription("Proxies to the Products service.")
.Produces<ProductDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapPost("/api/products", async (CreateProductRequest request, IProductsClient client) =>
{
    try
    {
        var product = await client.CreateProductAsync(request);
        return Results.Created($"/api/products/{product.Id}", product);
    }
    catch (Exception ex) { return HandleDownstreamError(ex, "Products service", logger); }
})
.WithName("CreateProduct")
.WithSummary("Create a product")
.WithDescription("Proxies to the Products service.")
.Produces<ProductDto>(StatusCodes.Status201Created);

app.MapPut("/api/products/{id:guid}", async (Guid id, UpdateProductRequest request, IProductsClient client) =>
{
    try
    {
        var product = await client.UpdateProductAsync(id, request with { Id = id });
        return Results.Ok(product);
    }
    catch (Refit.ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
    {
        return Results.NotFound();
    }
    catch (Exception ex) { return HandleDownstreamError(ex, "Products service", logger); }
})
.WithName("UpdateProduct")
.WithSummary("Update a product")
.WithDescription("Proxies to the Products service.")
.Produces<ProductDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapDelete("/api/products/{id:guid}", async (Guid id, IProductsClient client) =>
{
    try
    {
        await client.DeleteProductAsync(id);
        return Results.NoContent();
    }
    catch (Exception ex) { return HandleDownstreamError(ex, "Products service", logger); }
})
.WithName("DeleteProduct")
.WithSummary("Delete a product")
.WithDescription("Proxies to the Products service.")
.Produces(StatusCodes.Status204NoContent);

// ==================== Baskets routes ====================

app.MapGet("/api/baskets/{customerId}", async (string customerId, IBasketsClient client) =>
{
    try
    {
        var basket = await client.GetBasketAsync(customerId);
        return Results.Ok(basket);
    }
    catch (Exception ex) { return HandleDownstreamError(ex, "Baskets service", logger); }
})
.WithName("GetBasket")
.WithSummary("Get a basket by customer ID")
.WithDescription("Proxies to the Baskets service.")
.Produces<BasketDto>(StatusCodes.Status200OK);

app.MapPost("/api/baskets/{customerId}/items", async (string customerId, AddBasketItemRequest request, IBasketsClient client) =>
{
    try
    {
        var basket = await client.AddBasketItemAsync(customerId, request);
        return Results.Ok(basket);
    }
    catch (Exception ex) { return HandleDownstreamError(ex, "Baskets service", logger); }
})
.WithName("AddBasketItem")
.WithSummary("Add an item to the basket")
.WithDescription("Proxies to the Baskets service.")
.Produces<BasketDto>(StatusCodes.Status200OK);

app.MapDelete("/api/baskets/{customerId}/items/{productId:guid}", async (string customerId, Guid productId, IBasketsClient client) =>
{
    try
    {
        var basket = await client.RemoveBasketItemAsync(customerId, productId);
        return Results.Ok(basket);
    }
    catch (Exception ex) { return HandleDownstreamError(ex, "Baskets service", logger); }
})
.WithName("RemoveBasketItem")
.WithSummary("Remove an item from the basket")
.WithDescription("Proxies to the Baskets service.")
.Produces<BasketDto>(StatusCodes.Status200OK);

// ==================== Orders routes ====================

app.MapPost("/api/orders", async (SubmitOrderRequest request, IOrdersClient client) =>
{
    try
    {
        var order = await client.SubmitOrderAsync(request);
        return Results.Created($"/api/orders/{order.Id}", order);
    }
    catch (Exception ex) { return HandleDownstreamError(ex, "Orders service", logger); }
})
.WithName("SubmitOrder")
.WithSummary("Submit a new order")
.WithDescription("Proxies to the Orders service.")
.Produces<OrderDto>(StatusCodes.Status201Created);

app.MapGet("/api/orders/{id:guid}", async (Guid id, IOrdersClient client) =>
{
    try
    {
        var order = await client.GetOrderByIdAsync(id);
        return Results.Ok(order);
    }
    catch (Refit.ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
    {
        return Results.NotFound();
    }
    catch (Exception ex) { return HandleDownstreamError(ex, "Orders service", logger); }
})
.WithName("GetOrderById")
.WithSummary("Get an order by ID")
.WithDescription("Proxies to the Orders service.")
.Produces<OrderDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapGet("/api/orders", async (string customerId, IOrdersClient client) =>
{
    try
    {
        var orders = await client.GetOrdersByCustomerAsync(customerId);
        return Results.Ok(orders);
    }
    catch (Exception ex) { return HandleDownstreamError(ex, "Orders service", logger); }
})
.WithName("GetOrdersByCustomer")
.WithSummary("Get orders by customer")
.WithDescription("Proxies to the Orders service.")
.Produces<List<OrderDto>>(StatusCodes.Status200OK);

// ==================== Checkout saga orchestration ====================
// Orchestration-based saga: Step 1 reserves basket, Step 2 creates order, Step 3 completes.
// On failure, compensating actions run in reverse: cancel order (if created), restore basket items.

app.MapPost("/api/baskets/{customerId}/checkout", async (string customerId, CheckoutSagaOrchestrator saga, CancellationToken ct) =>
{
    var result = await saga.ExecuteAsync(customerId, ct);

    return result.FinalState switch
    {
        SagaStatus.Completed => Results.Ok(new
        {
            SagaId = result.SagaId,
            State = result.FinalState.ToString(),
            Order = result.Order,
            Message = "Checkout saga completed successfully."
        }),
        SagaStatus.Failed => Results.Problem(
            title: "Checkout saga failed — compensating actions executed",
            statusCode: StatusCodes.Status502BadGateway,
            detail: result.ErrorMessage ?? "Saga failed and compensation was applied."),
        _ => Results.Problem(
            title: "Checkout saga ended in unexpected state",
            statusCode: StatusCodes.Status500InternalServerError,
            detail: $"Saga ended in state: {result.FinalState}")
    };
})
.WithName("Checkout")
.WithSummary("Checkout basket via saga orchestration")
.WithDescription("Executes an orchestration-based saga: Step 1 checks out the basket (captures snapshot), Step 2 creates an order, Step 3 completes. On failure, compensating actions restore the basket and cancel the order. Saga state is persisted and queryable via GET /api/sagas/{id}.");

// ==================== Saga inspection ====================

app.MapGet("/api/sagas/{id:guid}", async (Guid id, CheckoutSagaOrchestrator saga, CancellationToken ct) =>
{
    var sagaState = await saga.GetSagaAsync(id, ct);
    return sagaState is not null ? Results.Ok(sagaState) : Results.NotFound();
})
.WithName("GetSagaById")
.WithSummary("Get saga state by ID")
.WithDescription("Returns the persisted state of a checkout saga, including current state, basket snapshot, order ID, and error message. Useful for demo visibility of saga state transitions.")
.Produces<SagaState>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.Run();
