using Contracts.Messaging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Products.Data;
using Products.Domain;
using Products.Features.Commands;
using Products.Features.Queries;

var builder = WebApplication.CreateBuilder(args);

// --- OpenTelemetry distributed tracing ---
builder.Services.AddOpenTelemetryTracing(builder.Configuration);

// --- EF Core InMemory ---
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseInMemoryDatabase("ProductsDb"));

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
        Title = "Products Service API",
        Version = "v1",
        Description = "Products/Catalog microservice with MediatR CQRS, EF Core InMemory, and RabbitMQ integration events."
    });
});

var app = builder.Build();

// --- Seed database on startup ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    await ProductDbSeeder.SeedAsync(db);
}

var logger = app.Logger;
app.Lifetime.ApplicationStarted.Register(() => logger.LogInformation("Products service started."));

// --- Swagger UI (available in all environments for this demo) ---
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Products Service API v1");
});

// --- Minimal API endpoints ---

/// <summary>
/// Get all products.
/// </summary>
/// <returns>A list of all products in the catalog.</returns>
/// <response code="200">Returns the list of products.</response>
app.MapGet("/api/products", async (IMediator mediator, CancellationToken ct) =>
{
    var products = await mediator.Send(new GetProductsQuery(), ct);
    return Results.Ok(products);
})
.WithName("GetProducts")
.WithSummary("Get all products")
.WithDescription("Returns all products in the catalog.")
.Produces<IEnumerable<Product>>(StatusCodes.Status200OK);

/// <summary>
/// Get a product by ID.
/// </summary>
/// <param name="id">The product ID.</param>
/// <returns>A single product.</returns>
/// <response code="200">Returns the product.</response>
/// <response code="404">Product not found.</response>
app.MapGet("/api/products/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    var product = await mediator.Send(new GetProductByIdQuery(id), ct);
    return product is not null ? Results.Ok(product) : Results.NotFound();
})
.WithName("GetProductById")
.WithSummary("Get a product by ID")
.WithDescription("Returns a single product by its unique identifier.")
.Produces<Product>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

/// <summary>
/// Create a new product.
/// </summary>
/// <param name="command">The product to create.</param>
/// <returns>The created product.</returns>
/// <response code="201">Product created successfully.</response>
app.MapPost("/api/products", async (CreateProductCommand command, IMediator mediator, CancellationToken ct) =>
{
    var product = await mediator.Send(command, ct);
    return Results.Created($"/api/products/{product.Id}", product);
})
.WithName("CreateProduct")
.WithSummary("Create a new product")
.WithDescription("Creates a new product and publishes a ProductChanged integration event.")
.Produces<Product>(StatusCodes.Status201Created);

/// <summary>
/// Update an existing product.
/// </summary>
/// <param name="id">The product ID.</param>
/// <param name="command">The updated product data.</param>
/// <returns>The updated product.</returns>
/// <response code="200">Product updated successfully.</response>
/// <response code="400">ID mismatch.</response>
/// <response code="404">Product not found.</response>
app.MapPut("/api/products/{id:guid}", async (Guid id, UpdateProductCommand command, IMediator mediator, CancellationToken ct) =>
{
    if (id != command.Id)
        return Results.BadRequest("ID in route must match ID in body.");

    var product = await mediator.Send(command with { Id = id }, ct);
    return product is not null ? Results.Ok(product) : Results.NotFound();
})
.WithName("UpdateProduct")
.WithSummary("Update an existing product")
.WithDescription("Updates a product and publishes a ProductChanged integration event.")
.Produces<Product>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound);

/// <summary>
/// Delete a product.
/// </summary>
/// <param name="id">The product ID.</param>
/// <response code="204">Product deleted successfully.</response>
/// <response code="404">Product not found.</response>
app.MapDelete("/api/products/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    var deleted = await mediator.Send(new DeleteProductCommand(id), ct);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteProduct")
.WithSummary("Delete a product")
.WithDescription("Deletes a product and publishes a ProductChanged integration event.")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

app.Run();
