using Contracts.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRabbitMqMessaging(builder.Configuration);

var app = builder.Build();

var logger = app.Logger;
app.Lifetime.ApplicationStarted.Register(() => logger.LogInformation("Notifications service started."));

app.MapGet("/", () => "Hello from Notifications");

app.Run();
