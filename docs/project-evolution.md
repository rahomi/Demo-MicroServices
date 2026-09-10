---
title: "Project Evolution — Chapter 19 Microservices Expansion"
created: "2026-09-09"
tags: [index, obsidian, evolution]
---

# 📊 Project Evolution — Chapter 19 Microservices Expansion

This is the **Obsidian index** for tracking the project's growth. Every ticket completion gets a note in `docs/notes/`. This index links them in dependency order so you can present the project's evolution as a narrative.

## 🗺️ Planning Artifacts

- [[PLAN]] — The 9-section plan (source of truth)
- [[MAP]] — Wayfinder map with decision tickets and blocking edges
- [[tickets]] — Tracer-bullet implementation tickets with acceptance criteria

## 📋 Decision Tickets (Wayfinder)

| Ticket | Title | Status | Completion Note |
|--------|-------|--------|-----------------|
| T01 | Verify .NET SDK and establish solution/project structure | ✅ Done | [[01-scaffold-solution]] |
| T02 | MediatR migration strategy for Products and Baskets | ⬜ Open | — |
| T03 | RabbitMQ contracts and connection infrastructure design | ✅ Done | [[03-rabbitmq-contracts-design]] |
| T04 | BFF routing map and Refit client contracts | ⬜ Open | — |
| T05 | Orders and Identity service design | ⬜ Open | — |
| T06 | Notifications service and event flow design | ⬜ Open | — |
| T07 | Docker Compose topology and port map | ⬜ Open | — |
| T08 | HTTP examples and README content | ⬜ Open | — |
| T09 | Saga orchestration and compensation design | ⬜ Open | — |
| T10 | OpenTelemetry + Jaeger distributed tracing design | ⬜ Open | — |

## 🚀 Implementation Tickets (Tracer Bullets)

| # | Ticket | Status | Completion Note |
|---|--------|--------|-----------------|
| 1 | Scaffold solution, projects, and shared Contracts | ✅ Done | [[01-scaffold-solution]] |
| 2 | Products service: MediatR CQRS + EF Core InMemory + CRUD + ProductChanged | ✅ Done | [[02-products-service]] |
| 3 | Baskets service: MediatR CQRS + EF Core InMemory + basket ops + BasketCheckedOut | ✅ Done | [[03-baskets-service]] |
| 4 | Identity service: fake customer endpoint | ✅ Done | [[04-identity-service]] |
| 5 | Orders service: MediatR CQRS + EF Core InMemory + order endpoints + OrderSubmitted | ✅ Done | [[05-orders-service]] |
| 6 | Notifications service: RabbitMQ consumer + received events endpoint | ⬜ Open | — |
| 7 | BFF: Refit clients + routing map + checkout orchestration | ⬜ Open | — |
| 8 | Saga pattern: orchestration-based checkout with compensating transactions | ⬜ Open | — |
| 9 | Distributed tracing: OpenTelemetry + Jaeger | ⬜ Open | — |
| 10 | Docker Compose: Dockerfiles + compose topology + RabbitMQ + Jaeger | ⬜ Open | — |
| 11 | HTTP examples + README documentation | ⬜ Open | — |

## 📝 Completion Notes (in order of completion)

> As each ticket is completed, create a note from the [template](templates/ticket-completion.md) and link it here. The notes form the project's evolution narrative.

1. [[01-scaffold-solution]] — Scaffold EcommerceDemo solution, projects, and shared Contracts (T01 + Ticket 1)
2. [[03-rabbitmq-contracts-design]] — RabbitMQ contracts and connection infrastructure design (T03)
3. [[02-products-service]] — Products service: MediatR CQRS + EF Core InMemory + CRUD + ProductChanged + Swagger UI (Ticket 2)
4. [[03-baskets-service]] — Baskets service: MediatR CQRS + EF Core InMemory + basket ops + BasketCheckedOut + ProductChanged consumer + Swagger UI (Ticket 3)
5. [[04-identity-service]] — Identity service: fake customer endpoint + Swagger UI (Ticket 4)
6. [[05-orders-service]] — Orders service: MediatR CQRS + EF Core InMemory + order endpoints + OrderSubmitted + Swagger UI (Ticket 5)

## 🏷️ Tag Legend

- `#ticket-completion` — A note documenting a completed ticket
- `#decision` — A note documenting a key architectural decision
- `#demo` — A note documenting a demo or presentation
- `#retrospective` — A note documenting lessons learned

## 📂 Folder Structure

```
docs/
├── project-evolution.md      ← THIS FILE (Obsidian index)
├── templates/
│   └── ticket-completion.md  ← Template for completion notes
├── notes/                    ← Completion notes go here
│   └── (T01-verify-sdk.md, T02-mediatr.md, etc.)
└── WORKFLOW.md               ← Developer workflow guide
```
