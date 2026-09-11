---
ticket: "10"
title: "Docker Compose: Dockerfiles + compose topology + RabbitMQ + Jaeger"
type: "task"
date_completed: "2026-09-11"
status: "completed"
blocked_by: [7, 6, 9]
blocks: [11]
tags: [ticket-completion]
---

# Ticket 10 — Docker Compose: Dockerfiles + compose topology + RabbitMQ + Jaeger

## Summary

Created multi-stage Dockerfiles for all six services and a complete `docker-compose.yml` topology with RabbitMQ (management UI + healthcheck), Jaeger (OTLP collector + UI), service discovery via container names, and environment-based configuration for RabbitMQ connections, OpenTelemetry export, and downstream service URLs. A user can run `docker compose up --build` and the entire stack starts with RabbitMQ and Jaeger ready before services connect.

## What was done

- Created 6 multi-stage Dockerfiles (one per service):
  - Build stage: `mcr.microsoft.com/dotnet/sdk:10.0` — restores, publishes
  - Runtime stage: `mcr.microsoft.com/dotnet/aspnet:10.0` — runs the published DLL
  - Each Dockerfile copies the Contracts project first (for restore caching), then the service project
- Created `docker-compose.yml` with 8 services:
  - **rabbitmq** — `rabbitmq:3-management`, ports 5672 (AMQP) + 15672 (management UI), healthcheck `rabbitmq-diagnostics -q ping`
  - **jaeger** — `jaegertracing/all-in-one:1.62`, `COLLECTOR_OTLP_ENABLED=true`, ports 16686 (UI) + 4317 (OTLP gRPC)
  - **bff** — port 5000, depends on RabbitMQ healthy, downstream URLs point to container names
  - **products** — port 5001, depends on RabbitMQ healthy
  - **baskets** — port 5002, depends on RabbitMQ healthy
  - **orders** — port 5003, depends on RabbitMQ healthy
  - **notifications** — port 5004, depends on RabbitMQ healthy
  - **identity** — port 5005, no RabbitMQ dependency (no messaging)
- Created `docker-compose.override.yml` with `ASPNETCORE_ENVIRONMENT=Development` and local port bindings
- Created `.dockerignore` to exclude bin/obj/.vs/.git from Docker build context
- All services use `depends_on` with `condition: service_healthy` for RabbitMQ
- Environment variables per service:
  - `ASPNETCORE_URLS` — sets the listening port inside the container
  - `RabbitMq__Host`, `RabbitMq__Port`, `RabbitMq__UserName`, `RabbitMq__Password` — RabbitMQ connection
  - `OTEL_SERVICE_NAME` — service identity in Jaeger
  - `OTEL_EXPORTER_OTLP_ENDPOINT` — points to `http://jaeger:4317`
  - `Downstream__*Url` (BFF only) — points to container names (e.g., `http://products:5001`)

## Key decisions

- **Port map:** BFF=5000, Products=5001, Baskets=5002, Orders=5003, Notifications=5004, Identity=5005. This follows the T07 decision ticket's port map and keeps the BFF on 5000 (existing convention).
- **Build context is `EcommerceDemo/` directory:** Each Dockerfile's build context is the `EcommerceDemo/` folder so the Contracts project is accessible. The `dockerfile` field points to the service-specific Dockerfile (e.g., `BFF/Dockerfile`).
- **Multi-stage Dockerfiles, per-service:** Each service gets its own Dockerfile for independence and clarity. All follow the same pattern: sdk build → runtime run.
- **Service discovery via container names:** Services reference each other by container name (e.g., `http://products:5001`). Docker's built-in DNS resolves container names on the default network.
- **Jaeger version pinned:** `jaegertracing/all-in-one:1.62` with `COLLECTOR_OTLP_ENABLED=true` to accept OTLP traces.
- **Identity has no RabbitMQ dependency:** Identity is a minimal service with no messaging, so it doesn't depend on RabbitMQ.

## Artifacts created

- `EcommerceDemo/BFF/Dockerfile` — Multi-stage Dockerfile for BFF service
- `EcommerceDemo/Products/Dockerfile` — Multi-stage Dockerfile for Products service
- `EcommerceDemo/Baskets/Dockerfile` — Multi-stage Dockerfile for Baskets service
- `EcommerceDemo/Orders/Dockerfile` — Multi-stage Dockerfile for Orders service
- `EcommerceDemo/Notifications/Dockerfile` — Multi-stage Dockerfile for Notifications service
- `EcommerceDemo/Identity/Dockerfile` — Multi-stage Dockerfile for Identity service
- `EcommerceDemo/.dockerignore` — Excludes bin/obj/.vs/.git from build context
- `docker-compose.yml` — Complete topology with 8 services
- `docker-compose.override.yml` — Local dev overrides (Development environment, port bindings)

## Testing & verification

- [x] Build verification — `dotnet build EcommerceDemo.slnx` — 0 errors, 0 warnings
- [ ] Docker Compose verification — Docker is not installed on this machine. `docker compose up --build` should be run on a machine with Docker installed.

```
dotnet build EcommerceDemo.slnx
Build succeeded. 0 Warning(s) 0 Error(s)
```

Expected verification on a Docker-enabled machine:
```bash
docker compose up --build
# RabbitMQ management UI: http://localhost:15672 (guest/guest)
# Jaeger UI: http://localhost:16686
# BFF Swagger: http://localhost:5000/swagger
```

## Dependencies

- **Blocked by:** [[07-bff-service]] (Ticket 7 — BFF), [[06-notifications-service]] (Ticket 6 — Notifications), [[09-distributed-tracing]] (Ticket 9 — Distributed tracing)
- **Unblocks:** [[11-http-examples-and-readme]] (Ticket 11 — HTTP examples + README)

## Notes for presentation

- One command starts the entire stack: `docker compose up --build`
- Show the RabbitMQ management UI at `http://localhost:15672` — queues for order-submitted, product-changed
- Show the Jaeger UI at `http://localhost:16686` — traces spanning BFF → Baskets → Orders → event → Notifications
- The healthcheck on RabbitMQ ensures services don't start until RabbitMQ is ready
- Service discovery is automatic via Docker DNS — no manual host configuration needed
- Each service runs in its own container with its own in-memory database, demonstrating data isolation

## Next steps

- **Ticket 11 (HTTP examples + README)** is now unblocked — all blockers (Docker Compose, Saga, Distributed tracing) are done. This is the final ticket.
