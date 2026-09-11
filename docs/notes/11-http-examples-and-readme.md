---
ticket: "11"
title: "HTTP examples and README documentation"
type: "task"
date_completed: "2026-09-11"
status: "completed"
blocked_by: ["10-docker-compose", "08-saga-orchestration", "09-distributed-tracing"]
blocks: []
tags: [ticket-completion]
---

# 11 — HTTP Examples and README Documentation

## Summary

Created a comprehensive `api.http` file covering all API operations (products, baskets, checkout, orders, event-driven behavior, saga failure, identity, and a full 10-step demo sequence) and a complete `README.md` with one-command startup instructions, service URLs, routing map, RabbitMQ/Jaeger access, sample request sequence, event flow explanation, saga pattern explanation, distributed tracing explanation, local dev instructions, in-memory reset limitations, and the 10-step verification checklist. A new user can now follow the README from clone to verified demo.

## What was done

- Created `api.http` at repo root with all API operations organized into 8 sections:
  1. Product operations (GET all, GET by ID, POST create, PUT update, DELETE)
  2. Basket operations (GET basket, POST add item, DELETE remove item)
  3. Checkout (saga orchestration + saga state inspection)
  4. Orders (POST submit, GET by ID, GET by customer, DELETE cancel)
  5. Event-driven behavior (trigger ProductChanged, check notifications)
  6. Saga failure & compensation (step-by-step instructions with docker compose stop/start)
  7. Identity (GET customer via BFF and direct)
  8. Full 10-step demo sequence walkthrough
- Created `README.md` at repo root covering all acceptance criteria:
  - One-command startup (`docker compose up --build`)
  - Service URLs table (all 6 services + RabbitMQ management + Jaeger UI)
  - Architecture overview with ASCII diagram and key design decisions
  - Routing map (BFF route → downstream service)
  - RabbitMQ management access instructions with event topology table
  - Jaeger UI access and sample trace walkthrough
  - Step-by-step sample request sequence (10 steps with curl commands)
  - Event flow explanation (checkout → BasketCheckedOut → OrderSubmitted → Notifications)
  - Saga pattern explanation and compensation flow with state machine diagram
  - Distributed tracing explanation (HTTP + RabbitMQ propagation)
  - In-memory database reset limitations
  - Local dev without Docker instructions
  - 10-step verification checklist from PLAN.md
  - Project structure tree
  - Technologies table

## Key decisions

- **Single `api.http` file at repo root:** Rather than scattering `.http` files across service projects, a single file at the repo root provides a complete, navigable API reference. It uses variables (`@bff`, `@products`, etc.) so requests can target either the BFF or a downstream service directly.
- **README covers both Docker and local dev:** The README includes instructions for `docker compose up --build` (primary) and `dotnet run` per service (for debugging), since PLAN.md requires both paths.
- **curl commands in README:** The sample request sequence uses `curl` commands so they can be copy-pasted in any terminal, independent of VS Code or the REST Client extension.

## Artifacts created

- `api.http` — HTTP examples for all API operations (products, baskets, checkout, orders, event-driven, saga failure, identity, full demo sequence)
- `README.md` — Comprehensive project documentation with startup, routing, tracing, saga, and verification sections

## Testing & verification

- [x] `dotnet build EcommerceDemo.slnx` — Build succeeded, 0 warnings, 0 errors
- [x] `git status` — Working tree clean after commit (only 2 new untracked files, no modifications to existing code)
- [x] Developer confirmed "yes" at the Step 5 gate

```
dotnet build EcommerceDemo.slnx
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Dependencies

- **Blocked by:** [[10-docker-compose]] (Docker Compose topology), [[08-saga-orchestration]] (Saga pattern), [[09-distributed-tracing]] (Distributed tracing)
- **Unblocks:** None — this is the final implementation ticket

## Notes for presentation

- The `api.http` file is the primary demo tool — open it in VS Code with the REST Client extension and walk through the sections in order.
- The README's 10-step sample request sequence mirrors PLAN.md's 10-step verification checklist, making it easy to demo the full flow.
- The saga failure section in `api.http` includes inline instructions for `docker compose stop orders` — this is the key "wow" moment for the demo.
- The distributed tracing section in the README explains how traces span both HTTP and RabbitMQ, which is a unique educational aspect of this demo.

## Next steps

- All 11 implementation tickets are now complete. The project is fully delivered.
- No further tickets are unblocked by this completion — this was the terminal ticket.
- Potential future enhancements (fog-of-war items from MAP.md): Baskets consuming `ProductChanged` for price sync (already implemented), retry/dead-letter queue strategy, health check endpoints, and seed data configuration.
