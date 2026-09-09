# T07 — Docker Compose topology and port map

**Labels:** `wayfinder:task`
**Parent:** [MAP — Chapter 19 Microservices Expansion](../MAP.md)
**Blocks:** T08
**Blocked by:** T01

## Question

What is the complete Docker Compose topology — services, ports, environment variables, dependencies, and RabbitMQ configuration — that keeps existing conventions while adding Orders, Notifications, Identity, and RabbitMQ?

### Detail

PLAN.md §5 requires adding Orders, Notifications, Identity, and RabbitMQ containers, configuring service discovery and environment-based RabbitMQ settings, and keeping existing BFF/Products/Baskets ports and Compose conventions. This ticket resolves:

1. **Port map** — Assign a port to every service:
   - BFF: `5000` (existing)
   - Products: `5001` (existing)
   - Baskets: `5002` (existing)
   - Orders: `5003` (new)
   - Notifications: `5004` (new)
   - Identity: `5005` (new)
   - RabbitMQ AMQP: `5672`
   - RabbitMQ Management: `15672`
2. **Dockerfile** — Each service needs a `Dockerfile`. Is it a shared multi-stage Dockerfile pattern, or per-service? (Multi-stage: `sdk` build → `runtime` run, per service.)
3. **docker-compose.yml** — Service definitions:
   - `bff`, `products`, `baskets`, `orders`, `notifications`, `identity`, `rabbitmq`
   - `depends_on` with healthchecks for RabbitMQ
   - Environment variables: `RABBITMQ__HOST`, `RABBITMQ__PORT`, `RABBITMQ__USERNAME`, `RABBITMQ__PASSWORD`, downstream service URLs for BFF
4. **docker-compose.override.yml** — Local port bindings and dev environment overrides.
5. **RabbitMQ container** — Official `rabbitmq:3-management` image. Healthcheck on `rabbitmq-diagnostics -q ping`.
6. **Service discovery** — Services reference each other by container name (e.g., `http://bff:5000`). BFF uses `http://products:5001`, etc.
7. **Build context** — Each service's build context is its project directory. Is there a shared base image or are they independent?

### Resolution

*(To be filled when resolved — record the port map, Dockerfile pattern, complete docker-compose.yml structure, and service discovery configuration.)*
