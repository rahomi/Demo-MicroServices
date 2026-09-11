# EcommerceDemo — .NET Microservices Demo

A locally runnable microservices demo built with **ASP.NET Core Minimal APIs**, **MediatR** (CQRS), **EF Core InMemory**, **direct RabbitMQ messaging**, **Docker Compose** orchestration, **orchestration-based Saga**, and **OpenTelemetry + Jaeger distributed tracing**.

This is an **educational demonstration** — not a production platform. Every service has its own ephemeral in-memory database, authentication is deliberately excluded, and the Identity service returns a fixed fake customer.

---

## Table of Contents

- [Quick Start](#quick-start)
- [Service URLs](#service-urls)
- [Architecture Overview](#architecture-overview)
- [Routing Map (BFF)](#routing-map-bff)
- [RabbitMQ Management](#rabbitmq-management)
- [Jaeger UI — Distributed Tracing](#jaeger-ui--distributed-tracing)
- [Sample Request Sequence](#sample-request-sequence)
- [Event Flow Explanation](#event-flow-explanation)
- [Saga Pattern & Compensation Flow](#saga-pattern--compensation-flow)
- [Distributed Tracing Explanation](#distributed-tracing-explanation)
- [Local Dev Without Docker](#local-dev-without-docker)
- [In-Memory Database Reset Limitations](#in-memory-database-reset-limitations)
- [10-Step Verification Checklist](#10-step-verification-checklist)

---

## Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (10.0.301 or later)
- [Docker](https://www.docker.com/get-started/) (with Docker Compose)

### One-Command Startup

```bash
docker compose up --build
```

This builds and starts all eight containers:

| Container | Description |
|-----------|-------------|
| `rabbitmq` | RabbitMQ broker + management UI |
| `jaeger` | Jaeger all-in-one (tracing backend + UI) |
| `bff` | Backend for Frontend (entry point, port 5000) |
| `products` | Products/Catalog service (port 5001) |
| `baskets` | Baskets service (port 5002) |
| `orders` | Orders service (port 5003) |
| `notifications` | Notifications service (port 5004) |
| `identity` | Identity service (port 5005) |

Wait ~30 seconds for RabbitMQ to become healthy and all services to connect. Then open the BFF Swagger UI:

```
http://localhost:5000/swagger
```

### Stop the stack

```bash
docker compose down
```

---

## Service URLs

| Service | URL | Swagger UI | Port |
|---------|-----|------------|------|
| BFF (entry point) | `http://localhost:5000` | `http://localhost:5000/swagger` | 5000 |
| Products | `http://localhost:5001` | `http://localhost:5001/swagger` | 5001 |
| Baskets | `http://localhost:5002` | `http://localhost:5002/swagger` | 5002 |
| Orders | `http://localhost:5003` | `http://localhost:5003/swagger` | 5003 |
| Notifications | `http://localhost:5004` | `http://localhost:5004/swagger` | 5004 |
| Identity | `http://localhost:5005` | `http://localhost:5005/swagger` | 5005 |
| RabbitMQ Management | `http://localhost:15672` | — | 15672 |
| Jaeger UI | `http://localhost:16686` | — | 16686 |

> **RabbitMQ credentials:** `guest` / `guest`

---

## Architecture Overview

```
                          ┌─────────────────────────────────────────────────┐
                          │                                                 │
   Client ──HTTP──→  BFF (port 5000)                                       │
                          │                                                 │
            ┌─────────────┼─────────────┬──────────────┐                   │
            ▼             ▼             ▼              ▼                    │
       Products       Baskets        Orders        Identity                 │
       (5001)         (5002)        (5003)        (5005)                   │
            │             │             │                                 │
            │             │             │                                 │
            └──────┬──────┘             │                                 │
                   │                    │                                 │
              RabbitMQ (5672) ──────────┘                                 │
                   │                                                      │
                   ▼                                                      │
             Notifications (5004)                                         │
                                                                        │
             Jaeger (16686) ←── OTLP traces from all services ────────────┘
```

### Key design decisions

- **CQRS with MediatR** — Products, Baskets, and Orders use command/query handlers via MediatR.
- **Direct RabbitMQ.Client** — No MassTransit. Event publishing and consuming use raw `RabbitMQ.Client` for educational visibility.
- **EF Core InMemory** — Each service has its own isolated in-memory database. Data resets on restart.
- **No authentication** — Identity returns a fixed fake customer (`cust-001`).
- **Saga orchestration** — Checkout is an orchestration-based saga in the BFF with compensating transactions.
- **OpenTelemetry** — Every service exports traces via OTLP to Jaeger. W3C `traceparent` headers propagate across HTTP and RabbitMQ.

---

## Routing Map (BFF)

All client requests go through the BFF on port 5000. The BFF routes to downstream services using Refit clients.

| BFF Route | Method | Downstream Service | Downstream Route |
|-----------|--------|--------------------|--------------------|
| `/api/products` | GET | Products | `/api/products` |
| `/api/products/{id}` | GET | Products | `/api/products/{id}` |
| `/api/products` | POST | Products | `/api/products` |
| `/api/products/{id}` | PUT | Products | `/api/products/{id}` |
| `/api/products/{id}` | DELETE | Products | `/api/products/{id}` |
| `/api/baskets/{customerId}` | GET | Baskets | `/api/baskets/{customerId}` |
| `/api/baskets/{customerId}/items` | POST | Baskets | `/api/baskets/{customerId}/items` |
| `/api/baskets/{customerId}/items/{productId}` | DELETE | Baskets | `/api/baskets/{customerId}/items/{productId}` |
| `/api/baskets/{customerId}/checkout` | POST | BFF Saga | (orchestrates Baskets + Orders) |
| `/api/orders` | POST | Orders | `/api/orders` |
| `/api/orders/{id}` | GET | Orders | `/api/orders/{id}` |
| `/api/orders?customerId={id}` | GET | Orders | `/api/orders?customerId={id}` |
| `/api/sagas/{id}` | GET | BFF (local) | (saga state inspection) |
| `/api/identity/customer` | GET | Identity | `/api/customer` |

> **Note:** The BFF prefixes Identity routes with `/api/identity/` while the downstream Identity service exposes `/api/customer`.

---

## RabbitMQ Management

The RabbitMQ management UI is available at:

```
http://localhost:15672
```

**Login:** `guest` / `guest`

### What to look for

1. **Queues tab** — See queues for each consumer:
   - `notifications.order-submitted` — Notifications consuming `OrderSubmitted`
   - `notifications.product-changed` — Notifications consuming `ProductChanged`
   - `baskets.product-changed` — Baskets consuming `ProductChanged`
2. **Exchanges tab** — The `amq.topic` exchange routes events using dot-separated routing keys (e.g., `orders.submitted`, `products.changed`, `baskets.checkedout`).
3. **Connections tab** — Each service maintains a persistent connection to RabbitMQ.

### Event topology

| Event | Routing Key | Publisher | Consumer(s) |
|-------|-------------|-----------|-------------|
| `OrderSubmitted` | `orders.submitted` | Orders service | Notifications |
| `ProductChanged` | `products.changed` | Products service | Baskets, Notifications |
| `BasketCheckedOut` | `baskets.checkedout` | Baskets service | (available for future consumers) |

---

## Jaeger UI — Distributed Tracing

The Jaeger UI is available at:

```
http://localhost:16686
```

### How to view a trace

1. Open Jaeger UI at `http://localhost:16686`.
2. In the **Search** tab, select a service from the dropdown (e.g., `bff`).
3. Click **Find Traces**.
4. Click any trace to expand the trace tree.

### What you'll see

A checkout request produces a single trace spanning multiple services:

```
BFF (checkout saga)
├── BFF → Baskets (HTTP: checkout)
├── BFF → Orders (HTTP: submit order)
├── Baskets → RabbitMQ (publish BasketCheckedOut)
├── Orders → RabbitMQ (publish OrderSubmitted)
└── Notifications ← RabbitMQ (consume OrderSubmitted)
```

- **HTTP spans** — BFF → Baskets and BFF → Orders calls appear as child spans (W3C `traceparent` propagation).
- **RabbitMQ spans** — Event publish and consume operations create linked spans using the `RabbitMQ` ActivitySource. The `traceparent` header is injected on publish and extracted on consume.
- **Service names** — Each service is tagged with `OTEL_SERVICE_NAME` (bff, products, baskets, orders, notifications, identity).

---

## Sample Request Sequence

A complete demo walkthrough using the [`api.http`](api.http) file or `curl`:

### Step 1 — Browse products

```bash
curl http://localhost:5000/api/products
```

Returns 5 seeded products (Wireless Mouse, Mechanical Keyboard, USB-C Hub, 27-inch Monitor, Laptop Stand).

### Step 2 — Get the fake customer

```bash
curl http://localhost:5000/api/identity/customer
```

Returns `{ "id": "cust-001", "name": "Test Customer", "email": "test@demo.local" }`.

### Step 3 — Add items to basket

```bash
curl -X POST http://localhost:5000/api/baskets/cust-001/items \
  -H "Content-Type: application/json" \
  -d '{"productId":"11111111-1111-1111-1111-111111111111","productName":"Wireless Mouse","unitPrice":29.99,"quantity":2}'
```

### Step 4 — View the basket

```bash
curl http://localhost:5000/api/baskets/cust-001
```

### Step 5 — Checkout (saga orchestration)

```bash
curl -X POST http://localhost:5000/api/baskets/cust-001/checkout
```

Returns the saga ID, state (`Completed`), and the created order.

### Step 6 — Verify the order

```bash
# Use the order ID from the checkout response
curl http://localhost:5000/api/orders/{orderId}
```

### Step 7 — Verify the basket is empty

```bash
curl http://localhost:5000/api/baskets/cust-001
```

### Step 8 — Check notifications

```bash
curl http://localhost:5004/api/notifications
```

You should see `BasketCheckedOut` and `OrderSubmitted` events consumed from RabbitMQ.

### Step 9 — Trigger a product change

```bash
curl -X PUT http://localhost:5000/api/products/11111111-1111-1111-1111-111111111111 \
  -H "Content-Type: application/json" \
  -d '{"id":"11111111-1111-1111-1111-111111111111","name":"Wireless Mouse Pro","price":34.99,"category":"Electronics"}'
```

Then check notifications again to see the `ProductChanged` event.

### Step 10 — View the distributed trace

Open `http://localhost:16686`, search for traces from the `bff` service, and expand a trace to see the full distributed trace.

---

## Event Flow Explanation

### Checkout flow

```
Client → BFF (POST /api/baskets/{customerId}/checkout)
                │
                ├── Saga Step 1: BFF → Baskets (checkout)
                │     └── Baskets publishes BasketCheckedOut → RabbitMQ
                │           └── Notifications consumes BasketCheckedOut
                │
                ├── Saga Step 2: BFF → Orders (submit order)
                │     └── Orders publishes OrderSubmitted → RabbitMQ
                │           └── Notifications consumes OrderSubmitted
                │
                └── Saga Step 3: Mark saga Completed
```

### Product change flow

```
Client → BFF → Products (PUT /api/products/{id})
                    └── Products publishes ProductChanged → RabbitMQ
                          ├── Baskets consumes ProductChanged (updates basket item names/prices)
                          └── Notifications consumes ProductChanged (stores in list)
```

### Integration events

| Event | Published by | Consumed by | Purpose |
|-------|-------------|-------------|---------|
| `BasketCheckedOut` | Baskets service | Notifications | Notifies that a basket was checked out |
| `OrderSubmitted` | Orders service | Notifications | Notifies that an order was created |
| `ProductChanged` | Products service | Baskets, Notifications | Keeps basket item names/prices in sync; records the change |

---

## Saga Pattern & Compensation Flow

The checkout is implemented as an **orchestration-based saga** in the BFF. The saga tracks state in an EF Core InMemory database (`SagaDb`).

### Saga steps

| Step | Action | Compensating Action |
|------|--------|---------------------|
| 1. BasketReserved | Call Baskets to checkout (clear basket, capture snapshot) | Restore basket items from snapshot |
| 2. OrderCreated | Call Orders to submit order | Cancel order (`DELETE /api/orders/{id}/cancel`) |
| 3. Completed | Mark saga as Completed | (none — terminal state) |

### Saga state machine

```
Started → BasketReserved → OrderCreated → Completed
    │          │               │
    │          │               └──→ Compensating → Failed
    │          └──→ Compensating → Failed
    └──→ Failed
```

### Compensation flow

If any step fails, the saga runs compensating actions **in reverse order**:

1. If Step 2 failed (order not created): **restore basket items** from the snapshot.
2. If Step 3 failed (order was created): **cancel the order** + **restore basket items**.

The saga is marked `Compensating` → `Failed`.

### Testing saga failure

```bash
# 1. Add items to the basket
curl -X POST http://localhost:5000/api/baskets/cust-001/items \
  -H "Content-Type: application/json" \
  -d '{"productId":"11111111-1111-1111-1111-111111111111","productName":"Wireless Mouse","unitPrice":29.99,"quantity":2}'

# 2. Stop the Orders container to simulate failure
docker compose stop orders

# 3. Trigger checkout — saga will compensate and restore the basket
curl -X POST http://localhost:5000/api/baskets/cust-001/checkout
# Expected: 502 Bad Gateway with "Checkout saga failed — compensating actions executed"

# 4. Verify the basket was restored
curl http://localhost:5000/api/baskets/cust-001
# Expected: basket contains the items again

# 5. Inspect the failed saga state (use the SagaId from step 3 response)
curl http://localhost:5000/api/sagas/{sagaId}
# Expected: CurrentState = "Failed", ErrorMessage = "Order creation failed; basket restored"

# 6. Restart the Orders container
docker compose start orders
```

### Viewing saga logs

```bash
docker compose logs bff | findstr "Saga"
```

Each saga step transition is logged with the saga ID and state.

---

## Distributed Tracing Explanation

Every service is instrumented with **OpenTelemetry** and exports traces via OTLP gRPC to the Jaeger container.

### How traces span HTTP and RabbitMQ

**HTTP propagation:**
- When the BFF calls a downstream service (e.g., Baskets), the OpenTelemetry HTTP client instrumentation automatically injects the W3C `traceparent` header.
- The downstream service's ASP.NET Core instrumentation extracts the header and creates a child span.
- Result: BFF → Baskets and BFF → Orders appear as parent-child spans in the same trace.

**RabbitMQ propagation:**
- When a service publishes an event, the `EventPublisher` injects the W3C `traceparent` into the RabbitMQ message headers (`IBasicProperties.Headers`).
- When a consumer receives the event, the `EventConsumer` extracts the `traceparent` and creates a linked activity.
- Result: The publish span and consume span are linked in the same trace tree.

**ActivitySource:**
- A shared `ActivitySource` named `"RabbitMQ"` is used for all publish and consume operations.
- Each service registers `AddSource("RabbitMQ")` to capture these spans.

### What a checkout trace looks like in Jaeger

```
span: BFF — POST /api/baskets/cust-001/checkout
├── span: BFF → Baskets — POST /api/baskets/cust-001/checkout (HTTP)
│   └── span: Baskets — RabbitMQ publish BasketCheckedOut
│       └── span: Notifications — RabbitMQ consume BasketCheckedOut
├── span: BFF → Orders — POST /api/orders (HTTP)
│   └── span: Orders — RabbitMQ publish OrderSubmitted
│       └── span: Notifications — RabbitMQ consume OrderSubmitted
```

---

## Local Dev Without Docker

If you want to run services individually (e.g., for debugging):

### 1. Start RabbitMQ

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### 2. Start Jaeger (optional, for tracing)

```bash
docker run -d --name jaeger -p 16686:16686 -p 4317:4317 -e COLLECTOR_OTLP_ENABLED=true jaegertracing/all-in-one:1.62
```

### 3. Run each service in a separate terminal

```bash
# Terminal 1 — Products
cd EcommerceDemo/Products
dotnet run

# Terminal 2 — Baskets
cd EcommerceDemo/Baskets
dotnet run

# Terminal 3 — Orders
cd EcommerceDemo/Orders
dotnet run

# Terminal 4 — Notifications
cd EcommerceDemo/Notifications
dotnet run

# Terminal 5 — Identity
cd EcommerceDemo/Identity
dotnet run

# Terminal 6 — BFF
cd EcommerceDemo/BFF
dotnet run
```

> **Note:** When running locally without Docker, the BFF's default downstream URLs point to `localhost` ports. Check `appsettings.json` in the BFF project for the default port configuration. You may need to adjust the `Downstream` section in `appsettings.Development.json` to match the ports each service listens on.

### 4. Build the entire solution

```bash
dotnet build EcommerceDemo.slnx
```

---

## In-Memory Database Reset Limitations

This demo uses **EF Core InMemory** databases. Important implications:

- **Data is ephemeral** — All data (products, baskets, orders, saga state) is lost when a service restarts.
- **Products re-seed on startup** — The Products service seeds 5 sample products every time it starts with an empty database.
- **No persistence across restarts** — Stopping and restarting a container resets that service's data to its initial state.
- **No shared state** — Each service has its own isolated in-memory database. There is no shared database.
- **Saga state is ephemeral** — Saga state in the BFF's `SagaDb` is lost on BFF restart.
- **Notifications are ephemeral** — The Notifications service stores received events in a `ConcurrentBag` in memory. Restarting the service clears the list.

> **To reset the entire demo to a clean state:** `docker compose down && docker compose up --build`

---

## 10-Step Verification Checklist

From `PLAN.md` — use this to verify the demo is working end-to-end:

- [ ] 1. **Build the solution:** `dotnet build EcommerceDemo.slnx` succeeds with zero errors
- [ ] 2. **Start the stack:** `docker compose up --build` — all 8 containers start
- [ ] 3. **Call BFF product endpoints:** `curl http://localhost:5000/api/products` returns seeded products
- [ ] 4. **Submit a checkout:** Add items to basket, then `POST /api/baskets/cust-001/checkout` — saga completes (basket reserved, order created, `OrderSubmitted` published)
- [ ] 5. **Verify Notifications:** `curl http://localhost:5004/api/notifications` — `OrderSubmitted` and `BasketCheckedOut` events appear
- [ ] 6. **Trigger a product change:** `PUT /api/products/{id}` — `ProductChanged` is published and consumed by Baskets and Notifications
- [ ] 7. **Trigger saga failure:** `docker compose stop orders`, then checkout — saga compensates and restores the basket
- [ ] 8. **View distributed trace:** Open `http://localhost:16686` — a checkout trace spans BFF → Baskets → Orders → RabbitMQ → Notifications
- [ ] 9. **Restart and verify reset:** `docker compose down && docker compose up --build` — all in-memory data resets, products re-seed
- [ ] 10. **Build and run independently:** Each service can be built and run on its own with `dotnet run` (requires local RabbitMQ)

---

## Project Structure

```
Demo-Microservices/
├── EcommerceDemo.slnx          # Solution file
├── Directory.Build.props        # Shared build settings (TreatWarningsAsErrors, TargetFramework)
├── docker-compose.yml           # All 8 services + RabbitMQ + Jaeger
├── api.http                     # HTTP examples for all API operations
├── PLAN.md                      # The 9-section plan (source of truth)
├── tickets.md                   # Tracer-bullet implementation tickets
├── docs/
│   ├── WORKFLOW.md              # Developer workflow guide
│   ├── project-evolution.md     # Obsidian index of completion notes
│   ├── notes/                   # Completion notes per ticket
│   └── templates/               # Ticket completion note template
├── tracker/
│   ├── MAP.md                   # Wayfinder map
│   └── tickets/                 # Decision ticket details
└── EcommerceDemo/
    ├── Contracts/               # Shared: event DTOs, RabbitMQ infra, OpenTelemetry extensions
    │   ├── Events/              # IntegrationEvents.cs (OrderSubmitted, ProductChanged, BasketCheckedOut)
    │   └── Messaging/           # IRabbitMqConnection, EventPublisher, EventConsumer, DI extensions
    ├── BFF/                     # Backend for Frontend (port 5000)
    │   ├── Clients/             # Refit client interfaces + DTOs
    │   └── Saga/                # CheckoutSagaOrchestrator, SagaState, SagaDbContext
    ├── Products/                # Products/Catalog service (port 5001)
    │   ├── Domain/              # Product entity
    │   ├── Data/                # ProductDbContext, ProductDbSeeder
    │   └── Features/            # Commands (Create/Update/Delete), Queries (GetAll/GetById)
    ├── Baskets/                 # Baskets service (port 5002)
    │   ├── Domain/              # Basket, BasketItem
    │   ├── Data/                # BasketDbContext
    │   └── Features/            # Commands, Queries, Consumers (ProductChanged)
    ├── Orders/                  # Orders service (port 5003)
    │   ├── Domain/              # Order, OrderItem
    │   ├── Data/                # OrderDbContext
    │   └── Features/            # Commands (Submit/Cancel), Queries (GetById/GetByCustomer)
    ├── Notifications/           # Notifications service (port 5004)
    │   ├── Consumers/           # OrderSubmittedConsumer, ProductChangedConsumer
    │   └── ReceivedEvents.cs    # In-memory event store
    └── Identity/               # Identity service (port 5005) — fake customer only
```

---

## Technologies

| Technology | Purpose |
|-----------|---------|
| ASP.NET Core 10 | Web API framework (Minimal APIs) |
| MediatR | CQRS command/query handler dispatch |
| EF Core InMemory | Ephemeral per-service databases |
| RabbitMQ.Client | Direct messaging (no MassTransit) |
| Refit | Type-safe HTTP clients in the BFF |
| OpenTelemetry | Distributed tracing instrumentation |
| Jaeger | Trace storage and UI |
| Docker Compose | Container orchestration |
| Swagger / OpenAPI | API documentation and testing UI |
