---
ticket: "T03"
title: "RabbitMQ contracts and connection infrastructure design"
type: "task"
date_completed: "2026-09-09"
status: "completed"
blocked_by: ["T01"]
blocks: ["T04", "T05", "T06"]
tags: [ticket-completion, decision]
---

# T03 — RabbitMQ Contracts and Connection Infrastructure Design

## Summary

Resolved the design decision for the shared Contracts project — event DTOs, RabbitMQ connection setup, and publish/consume abstractions using direct `RabbitMQ.Client`. The implementation was already done during T01 (scaffold); this ticket documents and confirms the design decisions so downstream tickets (T04, T05, T06) can proceed with a clear contract.

## What was done

- Read all Contracts source files to understand the actual implementation
- Documented the Resolution section in `tracker/tickets/T03-rabbitmq-contracts-and-connection-design.md` covering all 7 questions
- Updated `tracker/MAP.md` — marked T03 as resolved, moved T05 and T06 to the frontier (unblocked), updated the Blocked section
- Updated `docs/project-evolution.md` — marked T03 as ✅ Done
- Verified the solution still builds with zero errors and zero warnings

## Key decisions

- **Contracts project:** Class library `EcommerceDemo/Contracts/` targeting `net10.0`, referencing `RabbitMQ.Client 7.2.2` plus `Microsoft.Extensions.*` abstractions. No MassTransit — direct `RabbitMQ.Client` for educational visibility.
- **Event DTOs:** C# `record` types (`OrderSubmitted`, `ProductChanged`, `BasketCheckedOut` + `OrderItemDto`, `BasketItemDto`) in `Contracts.Events` namespace for immutability and value equality.
- **Connection:** `IRabbitMqConnection` / `RabbitMqConnection` — singleton in DI, semaphore-guarded lazy reconnect, no polling.
- **Publisher:** `IEventPublisher.PublishAsync<T>(exchange, routingKey, event)` — channel-per-publish, `System.Text.Json` serialization, injects W3C `traceparent` into message headers for distributed tracing.
- **Consumer:** `EventConsumer<T>` — abstract `BackgroundService` base class, subclasses implement `HandleAsync`, extracts `traceparent` from headers and creates linked activities, auto-ack/nack.
- **Topology:** `amq.topic` built-in topic exchange, dot-separated routing keys (`product.changed`, `order.submitted`, `basket.checkedout`), per-consumer durable queue names.
- **Serialization:** `System.Text.Json` with `JsonSerializer`.

## Artifacts created

- `tracker/tickets/T03-rabbitmq-contracts-and-connection-design.md` — Resolution section filled in (no code changes; documentation only)
- `tracker/MAP.md` — T03 marked resolved, frontier updated
- `docs/project-evolution.md` — T03 marked ✅ Done

## Testing & verification

- [x] `dotnet build EcommerceDemo.slnx` — Build succeeded, 0 warnings, 0 errors
- [x] `git status --short` — clean working tree after commit (only Obsidian IDE config files remain unstaged, unrelated to this ticket)

```
dotnet build EcommerceDemo.slnx --nologo -v q
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Dependencies

- **Blocked by:** [[01-scaffold-solution]] (T01 — scaffold created the Contracts project)
- **Unblocks:** [[05-orders-service]] (Orders service), [[06-notifications-service]] (Notifications service), [[07-bff-service]] (BFF routing)

## Notes for presentation

- This is a decision ticket, not an implementation ticket — the code was already written during T01
- The key value is documenting the design so downstream tickets have a clear contract to build against
- The `amq.topic` exchange + dot-separated routing keys pattern is simple and extensible
- Distributed tracing (W3C traceparent injection/extraction) is built into the publisher and consumer from the start, making T10 (OpenTelemetry + Jaeger) straightforward
- Show the Contracts project structure and the Resolution section in the T03 ticket

## Next steps

All downstream tickets have been completed:
- [[02-products-service]] — Products service (done)
- [[03-baskets-service]] — Baskets service (done)
- [[04-identity-service]] — Identity service (done)
- [[05-orders-service]] — Orders service (done)
- [[06-notifications-service]] — Notifications service (done)
- [[09-distributed-tracing]] — Distributed tracing (done, uses the traceparent infrastructure from T01)
