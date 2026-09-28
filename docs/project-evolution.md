---
title: "Project Evolution — EcommerceDemo Microservices"
created: "2026-09-09"
tags: [index, obsidian, evolution]
---

# 📊 Project Evolution — EcommerceDemo Microservices

This is the **Obsidian index** for the project's concept notes. Each note in `docs/notes/` covers a concept implemented in the EcommerceDemo microservices project — from solution scaffolding to saga orchestration and distributed tracing.

## 📝 Concept Notes (in order of implementation)

1. [[01-solution-scaffolding-and-contracts]] — Solution scaffolding, shared Contracts project, RabbitMQ infrastructure, W3C traceparent injection
2. [[02-rabbitmq-messaging-topology]] — RabbitMQ messaging topology design (amq.topic exchange, routing keys, queue naming, publisher/consumer contracts)
3. [[03-cqrs-with-mediatr]] — CQRS with MediatR, EF Core InMemory, CRUD endpoints, integration events, Swagger UI
4. [[04-basket-operations-and-event-consumer]] — Basket operations, RabbitMQ event consumer, EF Core InMemory change tracking gotchas
5. [[05-minimal-service-boundary]] — Minimal service boundary (intentionally simple service with no DB, no messaging)
6. [[06-order-submission-and-events]] — Order submission, server-side total calculation, OrderSubmitted event
7. [[07-event-driven-consumer-pattern]] — Event-driven consumer pattern, thread-safe in-memory store, pub-sub with separate queues
8. [[08-backend-for-frontend-pattern]] — Backend for Frontend pattern, Refit typed HTTP clients, routing map, checkout orchestration, error handling
9. [[09-saga-pattern-with-compensation]] — Saga orchestration pattern, state machine, compensating transactions, basket snapshot
10. [[10-distributed-tracing-with-opentelemetry]] — Distributed tracing with OpenTelemetry + Jaeger, W3C traceparent propagation across HTTP and RabbitMQ
11. [[11-docker-compose-and-containerization]] — Docker Compose, multi-stage Dockerfiles, service discovery, healthchecks, 9-container topology
12. [[12-api-documentation]] — API documentation with .http files and comprehensive README
13. [[13-frontend-architecture-and-ui]] — React storefront and admin UI, API routing, client state, and nginx integration

## 📂 Folder Structure

```
docs/
├── WORKFLOW.md
├── project-evolution.md      ← THIS FILE (Obsidian index)
├── notes/                    ← Numbered concept notes
│   ├── 01-solution-scaffolding-and-contracts.md
│   ├── 02-rabbitmq-messaging-topology.md
│   ├── 03-cqrs-with-mediatr.md
│   ├── 04-basket-operations-and-event-consumer.md
│   ├── 05-minimal-service-boundary.md
│   ├── 06-order-submission-and-events.md
│   ├── 07-event-driven-consumer-pattern.md
│   ├── 08-backend-for-frontend-pattern.md
│   ├── 09-saga-pattern-with-compensation.md
│   ├── 10-distributed-tracing-with-opentelemetry.md
│   ├── 11-docker-compose-and-containerization.md
│   ├── 12-api-documentation.md
│   └── 13-frontend-architecture-and-ui.md
├── superpowers/specs/        ← Design specifications
└── templates/                ← Note templates
```
