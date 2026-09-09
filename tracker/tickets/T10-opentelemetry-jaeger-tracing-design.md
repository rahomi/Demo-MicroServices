# T10 — OpenTelemetry + Jaeger distributed tracing design

**Labels:** `wayfinder:task`
**Parent:** [MAP — Chapter 19 Microservices Expansion](../MAP.md)
**Blocks:** T07, T08
**Blocked by:** T01

## Question

How is OpenTelemetry distributed tracing integrated into every service, and how is the Jaeger backend container configured so a user can see end-to-end traces spanning HTTP calls and RabbitMQ events?

### Detail

PLAN.md §9 adds distributed tracing with OpenTelemetry and Jaeger. This ticket resolves:

1. **NuGet packages** — Which OpenTelemetry packages does each service reference?
   - `OpenTelemetry.Extensions.Hosting`
   - `OpenTelemetry.Instrumentation.AspNetCore`
   - `OpenTelemetry.Instrumentation.Http`
   - `OpenTelemetry.Exporter.OpenTelemetryProtocol`
2. **Registration** — How is OpenTelemetry registered in each service's `Program.cs`?
   ```csharp
   builder.Services.AddOpenTelemetry()
       .WithTracing(t => t
           .AddAspNetCoreInstrumentation()
           .AddHttpClientInstrumentation()
           .AddSource("RabbitMQ")
           .AddOtlpExporter(o => o.Endpoint = new Uri(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"])));
   ```
3. **RabbitMQ activity source** — How are RabbitMQ publish/consume operations instrumented?
   - Create a shared `ActivitySource` named `"RabbitMQ"` in the Contracts project
   - `EventPublisher.PublishAsync` starts an activity (`activity.Start()`), tags event type and routing key
   - `EventConsumer` starts an activity on message receipt, links to the parent trace from message headers
4. **Trace context propagation across RabbitMQ** —
   - Publisher injects W3C `traceparent` header into RabbitMQ message properties (`IBasicProperties.Headers`)
   - Consumer extracts `traceparent` from message headers and creates a linked activity
   - This makes the trace continuous: BFF → Baskets → event publish → Notifications consume
5. **Jaeger container** —
   - `jaegertracing/all-in-one` image in Docker Compose
   - UI exposed on port 16686
   - OTLP gRPC on port 4317 (services send traces here)
   - Environment variable `OTEL_EXPORTER_OTLP_ENDPOINT=http://jaeger:4317` in each service
6. **Service identity** — Each service sets `OTEL_SERVICE_NAME` environment variable (e.g., `bff`, `products`, `baskets`) so traces are labeled correctly in Jaeger
7. **Local dev without Docker** — How does tracing work when running services with `dotnet run`? (Point `OTEL_EXPORTER_OTLP_ENDPOINT` to `http://localhost:4317` with a local Jaeger container)
8. **Sampling** — Is `AlwaysOn` sampler sufficient for a demo? (Yes — demo traffic is low.)

### Resolution

*(To be filled when resolved — record the OpenTelemetry registration snippet, RabbitMQ activity source design, trace context propagation mechanism, Jaeger container config, and service identity setup.)*
