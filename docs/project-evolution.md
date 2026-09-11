---
title: "Project Evolution — EcommerceDemo Microservices"
created: "2026-09-09"
tags: [index, obsidian, evolution]
---

# 📊 Project Evolution — EcommerceDemo Microservices

This is the **Obsidian index** for the project's concept notes. Each note in `docs/notes/` covers a concept implemented in the EcommerceDemo microservices project — from solution scaffolding to saga orchestration and distributed tracing.

## 📝 Concept Notes (in order of implementation)

1. [[solution-scaffolding-and-contracts]] — Solution scaffolding, shared Contracts project, RabbitMQ infrastructure, W3C traceparent injection
2. [[rabbitmq-messaging-topology]] — RabbitMQ messaging topology design (amq.topic exchange, routing keys, queue naming, publisher/consumer contracts)
3. [[cqrs-with-mediatr]] — CQRS with MediatR, EF Core InMemory, CRUD endpoints, integration events, Swagger UI
4. [[basket-operations-and-event-consumer]] — Basket operations, RabbitMQ event consumer, EF Core InMemory change tracking gotchas
5. [[minimal-service-boundary]] — Minimal service boundary (intentionally simple service with no DB, no messaging)
6. [[order-submission-and-events]] — Order submission, server-side total calculation, OrderSubmitted event
7. [[event-driven-consumer-pattern]] — Event-driven consumer pattern, thread-safe in-memory store, pub-sub with separate queues
8. [[backend-for-frontend-pattern]] — Backend for Frontend pattern, Refit typed HTTP clients, routing map, checkout orchestration, error handling
9. [[saga-pattern-with-compensation]] — Saga orchestration pattern, state machine, compensating transactions, basket snapshot
10. [[distributed-tracing-with-opentelemetry]] — Distributed tracing with OpenTelemetry + Jaeger, W3C traceparent propagation across HTTP and RabbitMQ
11. [[docker-compose-and-containerization]] — Docker Compose, multi-stage Dockerfiles, service discovery, healthchecks, 8-container topology
12. [[api-documentation]] — API documentation with .http files and comprehensive README

## 📂 Folder Structure

```
docs/
├── project-evolution.md      ← THIS FILE (Obsidian index)
├── WORKFLOW.md               ← Developer workflow guide
├── templates/                ← Note templates
└── notes/                    ← Concept notes
    ├── solution-scaffolding-and-contracts.md
    ├── rabbitmq-messaging-topology.md
    ├── cqrs-with-mediatr.md
    ├── basket-operations-and-event-consumer.md
    ├── minimal-service-boundary.md
    ├── order-submission-and-events.md
    ├── event-driven-consumer-pattern.md
    ├── backend-for-frontend-pattern.md
    ├── saga-pattern-with-compensation.md
    ├── distributed-tracing-with-opentelemetry.md
    ├── docker-compose-and-containerization.md
    └── api-documentation.md
```
