using Contracts.Messaging;
using Microsoft.OpenApi;
using Notifications;
using Notifications.Consumers;

var builder = WebApplication.CreateBuilder(args);

// --- OpenTelemetry distributed tracing ---
builder.Services.AddOpenTelemetryTracing(builder.Configuration);

// --- RabbitMQ messaging ---
builder.Services.AddRabbitMqMessaging(builder.Configuration);

// --- In-memory received events store (singleton, shared by consumers and endpoint) ---
builder.Services.AddSingleton<ReceivedEvents>();

// --- Event consumers (BackgroundService) ---
builder.Services.AddHostedService<OrderSubmittedConsumer>();
builder.Services.AddHostedService<ProductChangedConsumer>();

// --- OpenAPI / Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Notifications Service API",
        Version = "v1",
        Description = "Notifications microservice that consumes OrderSubmitted and ProductChanged events from RabbitMQ and exposes them via an endpoint."
    });
});

var app = builder.Build();

var logger = app.Logger;
app.Lifetime.ApplicationStarted.Register(() => logger.LogInformation("Notifications service started."));

// --- Swagger UI (available in all environments for this demo) ---
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Notifications Service API v1");
});

// --- Minimal API endpoints ---

app.MapGet("/api/notifications", (ReceivedEvents store) =>
{
    var events = store.GetAll();
    return Results.Ok(events);
})
.WithName("GetNotifications")
.WithSummary("Get all received notifications")
.WithDescription("Returns all integration events consumed from RabbitMQ (OrderSubmitted, ProductChanged), most recent first.")
.Produces<List<ReceivedEvent>>(StatusCodes.Status200OK);

app.Run();
