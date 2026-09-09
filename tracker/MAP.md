# MAP — Chapter 19 Microservices Expansion

**Labels:** `wayfinder:map`

## Destination

A locally runnable microservices demo extending Chapter 19: six ASP.NET Core services (BFF, Products, Baskets, Orders, Notifications, Identity) communicating via direct RabbitMQ messaging, each with its own EF Core InMemory database, orchestrated by Docker Compose, documented with `.http` examples and a README — a working educational demonstration, not a production platform.

## Notes

- **Domain:** .NET microservices, CQRS with MediatR, RabbitMQ integration events, Docker Compose orchestration.
- **Skills every session should consult:** `/grilling` and `/domain-modeling` for decision tickets; `/prototype` for any UI or API shape questions.
- **Standing preferences:**
  - Direct `RabbitMQ.Client` — no MassTransit, for educational visibility.
  - Authentication is deliberately excluded; Identity supplies a fake customer.
  - Persistence is ephemeral (EF Core InMemory), isolated per service.
  - Preserve the existing feature-folder and Minimal API structure.
  - Keep order creation synchronous for simple local testing; publish integration events asynchronously.
- **Source of truth:** `PLAN.md` at repo root.

## Decisions so far

*(No tickets resolved yet — the map is freshly charted.)*

## Ticket index

| Ticket | Title | Type | Blocked by | Blocks |
|--------|-------|------|------------|--------|
| [T01](tickets/T01-verify-sdk-and-solution-structure.md) | Verify .NET SDK and establish solution/project structure | research | — | T02, T03, T05, T07 |
| [T02](tickets/T02-mediatr-migration-strategy.md) | MediatR migration strategy for Products and Baskets | task | T01 | T04 |
| [T03](tickets/T03-rabbitmq-contracts-and-connection-design.md) | RabbitMQ contracts and connection infrastructure design | task | T01 | T04, T05, T06 |
| [T04](tickets/T04-bff-routing-and-refit-clients.md) | BFF routing map and Refit client contracts | task | T02, T03 | T08 |
| [T05](tickets/T05-orders-and-identity-service-design.md) | Orders and Identity service design | task | T01, T03 | T08 |
| [T06](tickets/T06-notifications-service-and-event-flow-design.md) | Notifications service and event flow design | task | T03 | T08 |
| [T07](tickets/T07-docker-compose-and-port-map.md) | Docker Compose topology and port map | task | T01, T10 | T08 |
| [T08](tickets/T08-http-examples-and-readme-content.md) | HTTP examples and README content | task | T04, T05, T06, T07 | — |
| [T09](tickets/T09-saga-orchestration-design.md) | Saga orchestration and compensation design | task | T04, T05 | T08 |
| [T10](tickets/T10-opentelemetry-jaeger-tracing-design.md) | OpenTelemetry + Jaeger distributed tracing design | task | T01 | T07, T08 |

### Frontier (open, unblocked, unclaimed)

- **T01** — Verify .NET SDK and establish solution/project structure

### Blocked (open, waiting on dependencies)

- T02 ← T01
- T03 ← T01
- T05 ← T01, T03
- T07 ← T01, T10
- T04 ← T02, T03
- T06 ← T03
- T09 ← T04, T05
- T10 ← T01
- T08 ← T04, T05, T06, T07, T09, T10

## Not yet specified

- **Baskets consuming `ProductChanged`** — Whether Baskets should update basket item prices when a product changes. This is a natural demo enhancement but PLAN.md doesn't require it. Will graduate as a ticket after T06 resolves the event flow.
- **Retry / dead-letter queue strategy** — What happens when a consumer fails to process an event? Out of scope for the initial demo but may become relevant if RabbitMQ integration proves fragile during T03/T06.
- **Health check endpoints** — Whether each service exposes `/health` for Docker Compose healthchecks beyond RabbitMQ. May graduate during T07.
- **Seed data** — What products are pre-seeded in the Products InMemory DB? What basket items? Will graduate during T05 or when implementation begins.

## Out of scope

- **Kubernetes** — Docker Compose is sufficient for a local demo.
- **Authentication providers** — Identity is a fake customer; no real auth.
- **Durable databases** — EF Core InMemory is intentional for ephemeral, isolated persistence.
- **Advanced retry / outbox pattern** — Direct publish/consume for educational visibility.
- **Production readiness** — The first deliverable is a working demonstration and README.

> **Note:** Saga pattern (T09) and distributed tracing (T10) were previously out of scope but have been moved in scope to enrich the educational demo. See PLAN.md §8 and §9.
