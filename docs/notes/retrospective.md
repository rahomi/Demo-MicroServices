---
title: "Project Retrospective — Chapter 19 Microservices Expansion"
date_completed: "2026-09-11"
status: "completed"
tags: [retrospective, concept-tutorial]
---

# 🔄 Project Retrospective — EcommerceDemo Microservices

> [!abstract]
> **Core Idea**
>
> What went well, what surprised us, what was harder than expected, and what we'd do differently. This is the honest look back after completing all 11 implementation tickets + 2 decision tickets.

---

## ✅ What Went Well

- **Tracer-bullet approach worked perfectly.** Each ticket cut a complete vertical slice (domain → data → MediatR → API → RabbitMQ → Swagger). Every ticket was independently demoable.

> [!tip]
> Embedding W3C `traceparent` injection/extraction in `EventPublisher` and `EventConsumer` during Ticket 1 meant the distributed tracing ticket (Ticket 9) was just SDK registration — no refactoring needed.

- **`TreatWarningsAsErrors` from day one.** Caught deprecated `WithOpenApi()` in .NET 10 and `NU1903` vulnerability warnings early.
- **Swagger UI on every service.** Made every service immediately testable without external tools.
- **Best-effort event publishing.** `try-catch` around `PublishAsync` let services run locally without RabbitMQ — essential since Docker wasn't available for most of the project.

---

## 😲 What Surprised Us

> [!warning]
> **EF Core InMemory `DbUpdateConcurrencyException`** — The Baskets service hit change tracking issues when using `Include(b => b.Items)` with `basket.Items.Clear()` across multiple requests. InMemory should behave like a toy, but it has real change tracking semantics. Fix: query `BasketItems` DbSet directly + explicit `BasketId` FK.

- **Swashbuckle vs `Microsoft.AspNetCore.OpenApi`** — .NET 10's built-in `WithOpenApi()` is deprecated and fails under `TreatWarningsAsErrors`. `OpenApiInfo` also moved from `Microsoft.OpenApi.Models` to `Microsoft.OpenApi` in 2.x.
- **RabbitMQ.Client 7.x API changes** — `DispatchConsumersAsync` no longer needed (async is default), `AsyncConsumerConsumerCancelledAsync` removed.
- **Saga naming collision** — Plan called for `SagaState` enum, but we also needed a `SagaState` entity. Renamed enum to `SagaStatus`.

---

## 😰 What Was Harder Than Expected

> [!danger]
> **Docker Compose without Docker.** Docker wasn't installed on the dev machine, so Docker Compose tickets (8, 9, 10) could only be build-verified, not runtime-verified. The `docker-compose.yml`, Dockerfiles, and healthchecks were written "blind."

- **Saga compensation design** — Getting the compensation flow right (reverse-order actions, best-effort error handling, snapshot capture timing) required careful thought.
- **OpenTelemetry package management** — Deciding where to put packages (Contracts vs per-service) and how to register consistently across 6 services required a shared extension method.

---

## 🔄 What We'd Do Differently

> [!info]
> **Install Docker earlier.** The inability to runtime-test Docker Compose, RabbitMQ event flow, and Jaeger traces was the biggest gap. Full-stack verification was deferred or skipped.

- **Add integration tests** — `WebApplicationFactory`-based tests would have caught the EF Core InMemory issues earlier.
- **Use `docker-compose.override.yml` more** — Could mount source code, add debug ports for local dev.
- **Consider a shared `appsettings.Docker.json`** — Reduce duplication across 6 `appsettings.json` files.

---

## 🏗️ Architecture Observations

- **BFF-as-gateway is clean for a demo.** All requests through one port (5000), BFF handles routing, error propagation, and saga orchestration.
- **Direct RabbitMQ.Client is educational but verbose.** `EventPublisher` and `EventConsumer<T>` hide complexity, but you still need to understand channels, exchanges, and routing keys. MassTransit would hide this — but the educational value is worth the verbosity.
- **InMemory databases are perfect for demos but misleading.** No constraints, no transactions, change tracking quirks. The `DbUpdateConcurrencyException` wouldn't happen with SQL Server.

---

## 🎬 Key Demo Moments

1. **Swagger UI on every service** — instant visual feedback
2. **Saga failure simulation** — `docker compose stop orders`, checkout → basket restored. The "wow" moment.
3. **Jaeger trace tree** — a single checkout spans BFF → Baskets → Orders → RabbitMQ → Notifications
4. **RabbitMQ management UI** — watching events appear in queues in real-time

---

## 📊 Stats

| Metric | Value |
|--------|-------|
| Tickets completed | 11 implementation + 2 decision = 13 |
| Services | 6 + 1 shared Contracts |
| Integration events | 3 (OrderSubmitted, ProductChanged, BasketCheckedOut) |
| NuGet packages | MediatR, EF Core InMemory, RabbitMQ.Client, Refit, Swashbuckle, OpenTelemetry (4) |
| Docker containers | 8 (6 services + RabbitMQ + Jaeger) |
| Build | 0 errors, 0 warnings (enforced by `TreatWarningsAsErrors`) |

---

## 📎 See Also

- [[01-scaffold-solution]] — Solution scaffolding + Contracts infrastructure
- [[02-products-service]] — CQRS with MediatR
- [[03-baskets-service]] — Event consumer + EF Core gotchas
- [[04-identity-service]] — Minimal service boundary
- [[05-orders-service]] — Server-side total calculation
- [[06-notifications-service]] — Event-driven consumer pattern
- [[07-bff-service]] — Backend for Frontend pattern
- [[08-saga-orchestration]] — Saga with compensating transactions
- [[09-distributed-tracing]] — OpenTelemetry + Jaeger
- [[10-docker-compose]] — Containerization + service discovery
- [[11-http-examples-and-readme]] — API documentation
- [[T03-rabbitmq-contracts-design]] — RabbitMQ topology design
