using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Contracts.Messaging;

/// <summary>
/// Shared OpenTelemetry registration for all microservices.
/// Instruments ASP.NET Core, HTTP clients, and the "RabbitMQ" activity source,
/// then exports traces via OTLP to a Jaeger (or any OTLP-compatible) backend.
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Registers OpenTelemetry tracing with ASP.NET Core, HttpClient, and RabbitMQ instrumentation.
    /// The OTLP endpoint and service name are read from configuration:
    ///   <list type="bullet">
    ///     <item><c>OTEL_EXPORTER_OTLP_ENDPOINT</c> — e.g. <c>http://localhost:4317</c> (local) or <c>http://jaeger:4317</c> (Docker)</item>
    ///     <item><c>OTEL_SERVICE_NAME</c> — e.g. <c>bff</c>, <c>products</c>, <c>baskets</c>, <c>orders</c>, <c>notifications</c>, <c>identity</c></item>
    ///   </list>
    /// If not set, the OTLP endpoint defaults to <c>http://localhost:4317</c> and the service name defaults to the application name.
    /// </summary>
    public static IServiceCollection AddOpenTelemetryTracing(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceName = configuration["OTEL_SERVICE_NAME"]
                          ?? configuration["ApplicationName"]
                          ?? "unknown-service";

        var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]
                           ?? "http://localhost:4317";

        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName))
            .WithTracing(t => t
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("RabbitMQ")
                .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)));

        return services;
    }
}
