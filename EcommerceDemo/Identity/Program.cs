using Contracts.Messaging;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// --- OpenTelemetry distributed tracing ---
builder.Services.AddOpenTelemetryTracing(builder.Configuration);

// --- OpenAPI / Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Identity Service API",
        Version = "v1",
        Description = "Minimal Identity microservice that returns a fixed fake customer. No database, no RabbitMQ, no MediatR — intentionally minimal."
    });
});

var app = builder.Build();

var logger = app.Logger;
app.Lifetime.ApplicationStarted.Register(() => logger.LogInformation("Identity service started."));

// --- Swagger UI (available in all environments for this demo) ---
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Service API v1");
});

// --- Minimal API endpoint ---

/// <summary>
/// Get the fake customer.
/// </summary>
/// <returns>A fixed fake customer object.</returns>
/// <response code="200">Returns the fake customer.</response>
app.MapGet("/api/customer", () => new
{
    Id = "cust-001",
    Name = "Test Customer",
    Email = "test@demo.local"
})
.WithName("GetCustomer")
.WithSummary("Get the fake customer")
.WithDescription("Returns a fixed fake customer. No authentication — this is a demo Identity service.")
.Produces<object>(StatusCodes.Status200OK);

app.Run();
