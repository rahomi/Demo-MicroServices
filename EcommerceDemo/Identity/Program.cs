var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var logger = app.Logger;
app.Lifetime.ApplicationStarted.Register(() => logger.LogInformation("Identity service started."));

app.MapGet("/", () => "Hello from Identity");

app.Run();
