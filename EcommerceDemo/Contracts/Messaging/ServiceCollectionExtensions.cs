using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Contracts.Messaging;

/// <summary>
/// DI registration extensions for RabbitMQ messaging infrastructure.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers IRabbitMqConnection (singleton), IEventPublisher, and the shared
    /// "RabbitMQ" ActivitySource for distributed tracing.
    /// </summary>
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IRabbitMqConnection>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMqConnection>>();
            var host = configuration["RabbitMq:Host"] ?? "localhost";
            var port = int.TryParse(configuration["RabbitMq:Port"], out var p) ? p : 5672;
            var user = configuration["RabbitMq:UserName"] ?? "guest";
            var pass = configuration["RabbitMq:Password"] ?? "guest";

            var factory = new ConnectionFactory
            {
                HostName = host,
                Port = port,
                UserName = user,
                Password = pass
            };

            return new RabbitMqConnection(factory, logger);
        });

        services.AddSingleton<IEventPublisher>(sp =>
        {
            var connection = sp.GetRequiredService<IRabbitMqConnection>();
            var logger = sp.GetRequiredService<ILogger<EventPublisher>>();
            return new EventPublisher(connection, logger);
        });

        return services;
    }
}
