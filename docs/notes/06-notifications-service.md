---
ticket: "6"
title: "Notifications service: RabbitMQ consumer + received events endpoint + Swagger UI"
type: "task"
date_completed: "2026-09-10"
status: "completed"
blocked_by: ["01-scaffold-solution"]
blocks: ["BFF: Refit clients + routing map + checkout orchestration", "Docker Compose: Dockerfiles + compose topology + RabbitMQ + Jaeger"]
tags: [ticket-completion]
---

# Ticket 6 — Notifications Service: RabbitMQ Consumer + Received Events Endpoint + Swagger UI

## Summary

Built the complete Notifications service that consumes `OrderSubmitted` and `ProductChanged` events from RabbitMQ, stores them in a thread-safe in-memory list, and exposes a `GET /api/notifications` endpoint to view them. Two `EventConsumer<T>` BackgroundServices are registered — one for each event type — that log each received event and store it with timestamp, event type, routing key, and JSON payload. Swagger UI is available at `/swagger`. A user can publish events (via Products or Orders services) and see them appear in the notifications list.

## What was done

- Added NuGet package: `Swashbuckle.AspNetCore 10.2.3`
- Created `ReceivedEvents` — thread-safe in-memory store using `ConcurrentBag<ReceivedEvent>`, registered as a singleton shared between consumers and the API endpoint
- Created `ReceivedEvent` record (EventType, RoutingKey, Payload, ReceivedAt)
- Created `OrderSubmittedConsumer : EventConsumer<OrderSubmitted>` — consumes `order.submitted` events from queue `notifications.order-submitted`, logs order details, stores in `ReceivedEvents`
- Created `ProductChangedConsumer : EventConsumer<ProductChanged>` — consumes `product.changed` events from queue `notifications.product-changed`, logs product details, stores in `ReceivedEvents`
- Registered both consumers as `IHostedService` via `AddHostedService`
- Created `GET /api/notifications` endpoint returning all received events, most recent first
- Added Swagger UI and OpenAPI specification at `/swagger` with `WithSummary`, `WithDescription`, and `Produces` metadata
- Updated `.http` file with example request
- Set service port to 5203 in `launchSettings.json`

## Key decisions

- **Singleton `ReceivedEvents` store:** The in-memory event store is a singleton (`ConcurrentBag<ReceivedEvent>`) shared between the consumer BackgroundServices (which write to it) and the API endpoint (which reads from it). This is intentional for the demo — no database is needed for ephemeral notification tracking.
- **Separate queues per event type:** Each consumer declares its own queue (`notifications.order-submitted`, `notifications.product-changed`) bound to `amq.topic` with the respective routing key. This means the Notifications service has its own copy of each event, independent of other consumers (e.g., Baskets' `ProductChanged` consumer).
- **No MediatR:** The Notifications service doesn't use MediatR — it has no commands or queries to handle. The consumers directly write to the `ReceivedEvents` store, and the endpoint directly reads from it. This keeps the service intentionally simple.
- **No database:** The Notifications service uses an in-memory list, not EF Core InMemory. Events are ephemeral and lost on restart — intentional for a notifications/demo service.
- **Swashbuckle.AspNetCore for Swagger UI:** Same decision as all other services — used Swashbuckle 10.2.3 for consistency and to avoid deprecation warnings under `TreatWarningsAsErrors`.
- **Full event flow testing deferred to Docker Compose:** Docker is not installed on the dev machine, so RabbitMQ can't be started locally. Level 1 testing (service starts, endpoint works, consumers start and retry) is sufficient. The full event flow (events actually consumed from RabbitMQ) will be verified during the Docker Compose ticket (Ticket 10).

## Artifacts created

- `EcommerceDemo/Notifications/ReceivedEvents.cs` — Thread-safe in-memory store and ReceivedEvent record
- `EcommerceDemo/Notifications/Consumers/OrderSubmittedConsumer.cs` — RabbitMQ consumer for OrderSubmitted events
- `EcommerceDemo/Notifications/Consumers/ProductChangedConsumer.cs` — RabbitMQ consumer for ProductChanged events
- `EcommerceDemo/Notifications/Program.cs` — Updated with RabbitMQ, consumers, ReceivedEvents singleton, Swagger UI, and GET /api/notifications endpoint
- `EcommerceDemo/Notifications/Notifications.csproj` — Added Swashbuckle.AspNetCore package
- `EcommerceDemo/Notifications/Properties/launchSettings.json` — Set port to 5203
- `EcommerceDemo/Notifications/Notifications.http` — Updated with example request

## Testing & verification

- [x] `dotnet build EcommerceDemo.slnx` — Build succeeded, 0 warnings, 0 errors
- [x] Service starts on `http://localhost:5203`
- [x] `GET /api/notifications` — Returns `[]` (empty list, expected — no RabbitMQ running) (200 OK)
- [x] Swagger UI accessible at `http://localhost:5203/swagger/index.html` (HTTP 200)
- [x] Both consumers registered as BackgroundServices, start on startup, retry RabbitMQ connection every 5s
- [x] Full event flow test (events consumed from RabbitMQ) — verified via Docker Compose in [[10-docker-compose]]

```
dotnet build EcommerceDemo.slnx --nologo
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Dependencies

- **Blocked by:** [[01-scaffold-solution]] — Scaffold solution, projects, and shared Contracts
- **Unblocks:** [[07-bff-service]] (full stack needs all services), [[10-docker-compose]] (all services must exist before Docker Compose)

## Notes for presentation

- Show the Swagger UI at `/swagger` — simple single-endpoint API
- Explain the event-driven architecture: Products publishes `ProductChanged`, Orders publishes `OrderSubmitted`, Notifications consumes both — no direct coupling between services
- Show the service logs — consumer startup messages and RabbitMQ connection retries demonstrate the messaging integration
- During Docker Compose demo: trigger a product create and an order submit, then call `GET /api/notifications` to see both events appear with timestamps
- Point out that each service has its own queue — Notifications gets its own copy of `ProductChanged` independent of Baskets
- The in-memory store resets on restart — intentional for ephemeral notifications

## Next steps

All downstream tickets have been completed:
- [[07-bff-service]] — BFF (done)
- [[08-saga-orchestration]] — Saga pattern (done)
- [[10-docker-compose]] — Docker Compose (done)
