---
ticket: "9"
title: "Distributed tracing: OpenTelemetry + Jaeger"
type: "task"
date_completed: "2026-09-11"
status: "completed"
blocked_by: [1]
blocks: [10, 11]
tags: [ticket-completion]
---

# Ticket 9 — Distributed tracing: OpenTelemetry + Jaeger

## Summary

Added OpenTelemetry distributed tracing to all six services (BFF, Products, Baskets, Orders, Notifications, Identity) so that a user can trigger a checkout and see a single end-to-end trace in Jaeger spanning HTTP calls (BFF → downstream services) and RabbitMQ events (event publish → Notifications consume). The RabbitMQ traceparent injection/extraction was already implemented in Ticket 1; this ticket wires up the OpenTelemetry SDK registration so those spans actually get exported to a Jaeger backend via OTLP.

## What was done

- Added 4 OpenTelemetry NuGet packages to the Contracts project:
  - `OpenTelemetry.Extensions.Hosting` (1.18.0)
  - `OpenTelemetry.Instrumentation.AspNetCore` (1.18.0)
  - `OpenTelemetry.Instrumentation.Http` (1.18.0)
  - `OpenTelemetry.Exporter.OpenTelemetryProtocol` (1.18.0)
- Created shared `OpenTelemetryExtensions.cs` in `Contracts/Messaging/` with `AddOpenTelemetryTracing()` extension method
- Registered OpenTelemetry in all 6 service `Program.cs` files by calling `builder.Services.AddOpenTelemetryTracing(builder.Configuration)`
- Added `OTEL_SERVICE_NAME` and `OTEL_EXPORTER_OTLP_ENDPOINT` to all 6 `appsettings.json` files
- The shared registration instruments:
  - ASP.NET Core (`AddAspNetCoreInstrumentation`) — traces incoming HTTP requests
  - HttpClient (`AddHttpClientInstrumentation`) — traces outgoing HTTP calls (BFF → downstream)
  - RabbitMQ activity source (`AddSource("RabbitMQ")`) — links to the existing ActivitySource in EventPublisher/EventConsumer
  - OTLP exporter (`AddOtlpExporter`) — sends traces to Jaeger at the configured endpoint

## Key decisions

- **Packages in Contracts only, not per-service:** All 6 service projects already reference Contracts. NuGet PackageReference packages flow transitively through project references, so adding them to Contracts alone is sufficient. This centralizes package version management in one place and avoids 24 duplicate `<PackageReference>` lines across 6 .csproj files.
- **Shared registration extension:** A single `AddOpenTelemetryTracing()` method in Contracts ensures all services are instrumented consistently. Each service just calls one line in its `Program.cs`.
- **Configuration via appsettings.json + environment variables:** `OTEL_SERVICE_NAME` and `OTEL_EXPORTER_OTLP_ENDPOINT` are set in appsettings.json for local dev defaults (`http://localhost:4317`). In Docker Compose, these will be overridden by environment variables (`http://jaeger:4317`).
- **RabbitMQ trace propagation already existed:** The `EventPublisher` (injects W3C traceparent into message headers) and `EventConsumer` (extracts traceparent, creates linked activity) were implemented in Ticket 1. This ticket adds the SDK registration so those ActivitySource spans get exported.
- **AlwaysOn sampling (implicit):** Demo traffic is low; default sampling is sufficient. No explicit sampler configuration needed.

## Artifacts created

- `EcommerceDemo/Contracts/Messaging/OpenTelemetryExtensions.cs` — Shared `AddOpenTelemetryTracing()` extension method
- `EcommerceDemo/Contracts/Contracts.csproj` — Added 4 OpenTelemetry package references
- `EcommerceDemo/BFF/Program.cs` — Added OpenTelemetry registration
- `EcommerceDemo/Products/Program.cs` — Added OpenTelemetry registration
- `EcommerceDemo/Baskets/Program.cs` — Added OpenTelemetry registration
- `EcommerceDemo/Orders/Program.cs` — Added OpenTelemetry registration
- `EcommerceDemo/Notifications/Program.cs` — Added OpenTelemetry registration
- `EcommerceDemo/Identity/Program.cs` — Added OpenTelemetry registration
- `EcommerceDemo/BFF/appsettings.json` — Added `OTEL_SERVICE_NAME: "bff"`, `OTEL_EXPORTER_OTLP_ENDPOINT`
- `EcommerceDemo/Products/appsettings.json` — Added `OTEL_SERVICE_NAME: "products"`, `OTEL_EXPORTER_OTLP_ENDPOINT`
- `EcommerceDemo/Baskets/appsettings.json` — Added `OTEL_SERVICE_NAME: "baskets"`, `OTEL_EXPORTER_OTLP_ENDPOINT`
- `EcommerceDemo/Orders/appsettings.json` — Added `OTEL_SERVICE_NAME: "orders"`, `OTEL_EXPORTER_OTLP_ENDPOINT`
- `EcommerceDemo/Notifications/appsettings.json` — Added `OTEL_SERVICE_NAME: "notifications"`, `OTEL_EXPORTER_OTLP_ENDPOINT`
- `EcommerceDemo/Identity/appsettings.json` — Added `OTEL_SERVICE_NAME: "identity"`, `OTEL_EXPORTER_OTLP_ENDPOINT`

## Testing & verification

- [x] Build verification — `dotnet build EcommerceDemo.slnx`
- [x] Zero errors, zero warnings

```
dotnet build EcommerceDemo.slnx
Build succeeded in 11.5s
```

- [ ] Full stack verification (Docker Compose + Jaeger) — will be verified in Ticket 10 (Docker Compose) when the Jaeger container is added
- [ ] Trace visibility in Jaeger UI — will be verified after Docker Compose ticket

## Dependencies

- **Blocked by:** [[01-scaffold-solution]] (Ticket 1 — scaffold + Contracts with RabbitMQ traceparent injection)
- **Unblocks:** [[10-docker-compose]] (Ticket 10 — Docker Compose with Jaeger container), [[11-http-examples-and-readme]] (Ticket 11 — README with Jaeger UI access instructions)

## Notes for presentation

- The RabbitMQ trace propagation was built in Ticket 1 but had no backend to export to. This ticket completes the tracing story by adding the OpenTelemetry SDK.
- Key demo: trigger a checkout, open Jaeger UI at `http://localhost:16686`, and show a single trace tree: BFF → Baskets → Orders → event publish → Notifications consume.
- The trace crosses the async messaging boundary because `EventPublisher` injects the W3C `traceparent` header into RabbitMQ message properties, and `EventConsumer` extracts it to create a linked activity.
- Emphasize the transitive package reference design — one place to manage OpenTelemetry versions.
- The Jaeger container itself is added in Ticket 10 (Docker Compose). For local dev without Docker, run a standalone Jaeger container: `docker run -d -p 4317:4317 -p 16686:16686 jaegertracing/all-in-one`.

## Next steps

- **Ticket 10 (Docker Compose)** is now unblocked — add Jaeger container (`jaegertracing/all-in-one`, ports 16686 + 4317) and wire up `OTEL_EXPORTER_OTLP_ENDPOINT=http://jaeger:4317` environment variables per service.
- **Ticket 11 (HTTP examples + README)** is partially unblocked — can document Jaeger UI access and sample trace walkthrough, but needs Docker Compose and saga tickets complete first.
- **T10 decision ticket** can now be marked resolved in `tracker/MAP.md`.
