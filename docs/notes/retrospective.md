---
title: "Project Retrospective — Chapter 19 Microservices Expansion"
date_completed: "2026-09-11"
status: "completed"
tags: [retrospective]
---

# 🔄 Project Retrospective — EcommerceDemo Microservices

## What went well

- **Tracer-bullet approach worked perfectly.** Each ticket cut a complete vertical slice through all layers (domain → data → MediatR → API → RabbitMQ → Swagger). Every ticket was independently demoable.
- **Contracts project built in from the start.** Embedding W3C `traceparent` injection/extraction in `EventPublisher` and `EventConsumer` during Ticket 1 meant the distributed tracing ticket (Ticket 9) was just SDK registration — no refactoring needed.
- **`TreatWarningsAsErrors` from day one.** Enforcing zero warnings from the first commit caught issues early (e.g., deprecated `WithOpenApi()` in .NET 10, `NU1903` vulnerability warnings) and kept the codebase clean.
- **Swagger UI on every service.** Adding Swagger from Ticket 2 onward made every service immediately testable without external tools. This accelerated development and demoing.
- **Best-effort event publishing.** Wrapping `IEventPublisher.PublishAsync` in try-catch let services run locally without RabbitMQ, which was essential since Docker wasn't available on the dev machine for most of the project.

## What surprised us

- **EF Core InMemory `DbUpdateConcurrencyException`.** The Baskets service hit change tracking issues when using `Include(b => b.Items)` with `basket.Items.Add()` / `basket.Items.Clear()` across multiple requests. The fix was to query the `BasketItems` DbSet directly and use an explicit `BasketId` FK instead of a shadow foreign key. This was unexpected — InMemory should behave like a toy, but it has real change tracking semantics.
- **Swashbuckle vs `Microsoft.AspNetCore.OpenApi`.** .NET 10's built-in `WithOpenApi()` extension is deprecated and fails under `TreatWarningsAsErrors`. Had to use Swashbuckle.AspNetCore 10.2.3 instead. The `OpenApiInfo` class also moved from `Microsoft.OpenApi.Models` to `Microsoft.OpenApi` in OpenApi 2.x.
- **RabbitMQ.Client 7.x API changes.** `DispatchConsumersAsync` is no longer needed (async is default in 7.x), and `AsyncConsumerConsumerCancelledAsync` was removed. The connection class had to be adapted.
- **Saga state machine naming collision.** The plan called for a `SagaState` enum, but we also needed a `SagaState` entity class. Renamed the enum to `SagaStatus` — a small but important distinction.

## What was harder than expected

- **Docker Compose without Docker.** Docker wasn't installed on the dev machine, so Docker Compose tickets (8, 9, 10) could only be build-verified, not runtime-verified. The `docker-compose.yml`, Dockerfiles, and healthchecks were written "blind" and need validation on a Docker-enabled machine.
- **Saga compensation design.** Getting the compensation flow right (reverse-order compensating actions, best-effort error handling, snapshot capture timing) required careful thought. The `Compensating` → `Failed` state transition and the "what if compensation itself fails?" question added complexity.
- **OpenTelemetry package management.** Deciding where to put the packages (Contracts vs per-service) and how to register consistently across 6 services required a shared extension method (`AddOpenTelemetryTracing`). The transitive package reference approach worked but required understanding of how NuGet flows through project references.

## What we'd do differently

- **Install Docker earlier.** The inability to runtime-test Docker Compose, RabbitMQ event flow, and Jaeger traces was the biggest gap. Full-stack verification (Level 4–5 testing) was deferred or skipped.
- **Add integration tests.** The project relies entirely on manual testing via Swagger/curl. A few integration tests (e.g., `WebApplicationFactory`-based tests for each service) would have caught the EF Core InMemory issues earlier and given confidence for the Docker tickets.
- **Use `docker-compose.override.yml` for local dev overrides.** The override file was created but could be more useful for local development (e.g., mounting source code, debug ports).
- **Consider a shared `appsettings.Docker.json`.** Each service has its own `appsettings.json` with OpenTelemetry config. A shared configuration approach would reduce duplication.

## Architecture observations

- **The BFF-as-gateway pattern is clean for a demo.** All client requests go through one port (5000), and the BFF handles routing, error propagation, and saga orchestration. This is simple to demo and understand.
- **Direct RabbitMQ.Client is educational but verbose.** The `EventPublisher` and `EventConsumer<T>` abstractions in Contracts hide the complexity, but a reader still needs to understand channels, exchanges, routing keys, and queue bindings. MassTransit would hide this, but the educational value of seeing the raw API is worth the verbosity.
- **InMemory databases are perfect for demos but misleading.** They don't enforce constraints, don't support transactions, and have change tracking quirks. The `DbUpdateConcurrencyException` in Baskets wouldn't happen with SQL Server or SQLite. This is fine for a demo but should be called out.

## Key demo moments

1. **Swagger UI on every service** — instant visual feedback, no setup needed
2. **Saga failure simulation** — `docker compose stop orders`, then checkout → basket restored. This is the "wow" moment.
3. **Jaeger trace tree** — a single checkout produces a trace spanning BFF → Baskets → Orders → RabbitMQ → Notifications. Seeing the async messaging boundary in a trace is powerful.
4. **RabbitMQ management UI** — watching events appear in queues in real-time makes the event-driven architecture tangible.

## Stats

- **Tickets completed:** 11 implementation + 2 decision (T01, T03) = 13 total
- **Services:** 6 (BFF, Products, Baskets, Orders, Notifications, Identity) + 1 shared Contracts
- **Integration events:** 3 (OrderSubmitted, ProductChanged, BasketCheckedOut)
- **NuGet packages:** MediatR, EF Core InMemory, RabbitMQ.Client, Refit, Swashbuckle, OpenTelemetry (4 packages)
- **Docker containers:** 8 (6 services + RabbitMQ + Jaeger)
- **Build:** 0 errors, 0 warnings (enforced by `TreatWarningsAsErrors`)
