# Tickets: Chapter 19 Microservices Expansion

Breaks `PLAN.md` into tracer-bullet vertical slices. Each slice cuts a complete path through all layers and is demoable on its own. Reference the [Wayfinder MAP](tracker/MAP.md) for the decision tickets that gate these.

Work the **frontier**: any ticket whose blockers are all done. For a purely linear chain that means top to bottom.

---

## Scaffold solution, projects, and shared Contracts

**What to build:** The .NET solution with all seven projects (BFF, Products, Baskets, Orders, Notifications, Identity, Contracts), `Directory.Build.props` for shared build settings, and the Contracts project containing integration event DTOs (`OrderSubmitted`, `ProductChanged`, `BasketCheckedOut`) plus RabbitMQ connection, publisher, and consumer abstractions using direct `RabbitMQ.Client`. Every service project gets a minimal `Program.cs` that starts an empty web server. The deliverable is a green `dotnet build` of the entire solution.

**Blocked by:** None — can start immediately.

- [ ] `dotnet --list-sdks` run; target framework selected and recorded
- [ ] `C19.sln` created at repo root
- [ ] `Directory.Build.props` with shared `<TargetFramework>`, `<Nullable>`, `<ImplicitUsings>`
- [ ] Six web API projects + one class library created and added to solution
- [ ] Contracts project: event DTOs (`OrderSubmitted`, `ProductChanged`, `BasketCheckedOut`)
- [ ] Contracts project: `IRabbitMqConnection`, `RabbitMqConnection` (persistent connection with reconnect)
- [ ] Contracts project: `IEventPublisher` interface and `EventPublisher` implementation
- [ ] Contracts project: `IEventConsumer` / `EventConsumer` base class or `BackgroundService` pattern
- [ ] Contracts project: `RabbitMQ.Client` NuGet package added
- [ ] Contracts project: shared `ActivitySource` named `"RabbitMQ"` for distributed tracing
- [ ] Each service project has a minimal `Program.cs` that returns "Hello from {service}"
- [ ] `dotnet build C19.sln` succeeds with zero errors

---

## Products service: MediatR CQRS + EF Core InMemory + CRUD + ProductChanged event

**What to build:** The complete Products/Catalog service with MediatR command/query handlers, EF Core InMemory database with seed data, full CRUD endpoints via Minimal API, and `ProductChanged` event publishing on create/update/delete. A user can start the service, browse seeded products, create/update/delete products, and see `ProductChanged` events published to RabbitMQ.

**Blocked by:** Scaffold solution, projects, and shared Contracts

- [ ] `ProductDbContext : DbContext` with `DbSet<Product>` using `UseInMemoryDatabase("ProductsDb")`
- [ ] Seed data: 3–5 sample products on startup
- [ ] MediatR registered in `Program.cs` (`AddMediatR`)
- [ ] Query handlers: `GetProducts`, `GetProductById`
- [ ] Command handlers: `CreateProduct`, `UpdateProduct`, `DeleteProduct`
- [ ] `ProductChanged` event published on create, update, and delete via `IEventPublisher`
- [ ] Minimal API endpoints: `GET /api/products`, `GET /api/products/{id}`, `POST /api/products`, `PUT /api/products/{id}`, `DELETE /api/products/{id}`
- [ ] RabbitMQ connection registered in DI, publisher injected into handlers
- [ ] Service runs and all endpoints return correct responses
- [ ] `ProductChanged` events visible in RabbitMQ management UI or logs

---

## Baskets service: MediatR CQRS + EF Core InMemory + basket ops + BasketCheckedOut event

**What to build:** The complete Baskets service with MediatR handlers, EF Core InMemory database, basket item add/remove endpoints, a checkout endpoint that clears the basket and publishes `BasketCheckedOut`, and consumes `ProductChanged` events to keep basket item names/prices in sync. A user can create a basket, add items, remove items, and checkout — with `BasketCheckedOut` published to RabbitMQ.

**Blocked by:** Scaffold solution, projects, and shared Contracts

- [ ] `BasketDbContext : DbContext` with `DbSet<Basket>`, `DbSet<BasketItem>` using `UseInMemoryDatabase("BasketsDb")`
- [ ] MediatR registered in `Program.cs`
- [ ] Query handlers: `GetBasket` (by customer ID)
- [ ] Command handlers: `AddBasketItem`, `RemoveBasketItem`, `CheckoutBasket`
- [ ] `CheckoutBasket` handler clears basket items and publishes `BasketCheckedOut` event
- [ ] `ProductChanged` consumer updates basket item names/prices when products change
- [ ] Minimal API endpoints: `GET /api/baskets/{customerId}`, `POST /api/baskets/{customerId}/items`, `DELETE /api/baskets/{customerId}/items/{productId}`, `POST /api/baskets/{customerId}/checkout`
- [ ] RabbitMQ connection, publisher, and consumer registered in DI
- [ ] Service runs and all endpoints return correct responses
- [ ] `BasketCheckedOut` event visible in RabbitMQ on checkout

---

## Identity service: fake customer endpoint

**What to build:** A minimal Identity service that returns a fixed fake customer via a single endpoint. No database, no MediatR — just a static response. A user can call `GET /api/customer` and receive the fake customer JSON.

**Blocked by:** Scaffold solution, projects, and shared Contracts

- [ ] `GET /api/customer` endpoint returning `{ Id: "cust-001", Name: "Test Customer", Email: "test@demo.local" }`
- [ ] Service runs and endpoint returns correct response
- [ ] No database, no RabbitMQ, no MediatR — intentionally minimal

---

## Orders service: MediatR CQRS + EF Core InMemory + order endpoints + OrderSubmitted event

**What to build:** The complete Orders service with MediatR handlers, EF Core InMemory database, order submission (creates order, publishes `OrderSubmitted`), and order query endpoints. A user can submit an order and retrieve it by ID or by customer. `OrderSubmitted` is published to RabbitMQ on order creation.

**Blocked by:** Scaffold solution, projects, and shared Contracts

- [ ] `OrderDbContext : DbContext` with `DbSet<Order>` using `UseInMemoryDatabase("OrdersDb")`
- [ ] Domain model: `Order` (Id, CustomerId, Items, Total, Status, CreatedAt), `OrderItem` (ProductId, ProductName, UnitPrice, Quantity)
- [ ] MediatR registered in `Program.cs`
- [ ] Command handler: `SubmitOrder` (creates order, publishes `OrderSubmitted`, returns order)
- [ ] Query handlers: `GetOrderById`, `GetOrdersByCustomer`
- [ ] Minimal API endpoints: `POST /api/orders`, `GET /api/orders/{id}`, `GET /api/orders?customerId={id}`
- [ ] RabbitMQ connection and publisher registered in DI
- [ ] Service runs and all endpoints return correct responses
- [ ] `OrderSubmitted` event visible in RabbitMQ on order creation

---

## Notifications service: RabbitMQ consumer + received events endpoint

**What to build:** The Notifications service that consumes `OrderSubmitted` and `ProductChanged` events from RabbitMQ, stores received events in an in-memory list, and exposes a `GET /api/notifications` endpoint to view them. A user can publish events (manually or via other services) and see them appear in the notifications list.

**Blocked by:** Scaffold solution, projects, and shared Contracts

- [ ] `BackgroundService` / `IHostedService` consumer registered on startup
- [ ] Consumes `OrderSubmitted` events → logs and stores in-memory list
- [ ] Consumes `ProductChanged` events → logs and stores in-memory list
- [ ] `GET /api/notifications` endpoint returns list of received events with timestamp
- [ ] RabbitMQ connection and consumer registered in DI
- [ ] Service runs, consumes events from RabbitMQ, and `GET /api/notifications` returns received events
- [ ] Events visible in service logs and via the endpoint

---

## BFF: Refit clients + routing map + checkout orchestration

**What to build:** The Backend for Frontend service with Refit client interfaces for all downstream services (Products, Baskets, Orders, Identity), a complete routing map exposing all endpoints through the BFF, and checkout orchestration that calls Baskets to checkout and Orders to create an order. A user can call any endpoint through the BFF on port 5000 and it routes to the correct downstream service. (Saga orchestration is added in a later ticket — this ticket delivers the basic synchronous checkout first.)

**Blocked by:** Products service, Baskets service, Identity service, Orders service

- [ ] Refit client interfaces: `IProductsClient`, `IBasketsClient`, `IOrdersClient`, `IIdentityClient`
- [ ] Refit clients registered in DI with configurable base addresses from environment/config
- [ ] BFF endpoints proxying to Products: `GET/POST/PUT/DELETE /api/products...`
- [ ] BFF endpoints proxying to Baskets: `GET/POST/DELETE /api/baskets...`
- [ ] BFF endpoints proxying to Orders: `GET/POST /api/orders...`
- [ ] BFF endpoint proxying to Identity: `GET /api/identity/customer`
- [ ] Checkout orchestration: `POST /api/baskets/{customerId}/checkout` → calls Baskets checkout, then calls Orders to submit order, returns order
- [ ] Downstream service URLs configurable via environment variables
- [ ] Error propagation: downstream errors surfaced with appropriate status codes
- [ ] BFF runs on port 5000 and all routes work end-to-end

---

## Docker Compose: Dockerfiles + compose topology + RabbitMQ + Jaeger

**What to build:** Multi-stage Dockerfiles for each service, a `docker-compose.yml` with all seven services plus RabbitMQ (with management UI) and Jaeger (for distributed tracing), healthchecks, service discovery via container names, and a `docker-compose.override.yml` for local port bindings. A user can run `docker compose up --build` and the entire stack starts with RabbitMQ and Jaeger ready before services connect.

**Blocked by:** BFF: Refit clients + routing map + checkout orchestration, Notifications service, Distributed tracing: OpenTelemetry + Jaeger

- [ ] Multi-stage `Dockerfile` for each of the six service projects (sdk build → runtime run)
- [ ] `docker-compose.yml` with services: `bff`, `products`, `baskets`, `orders`, `notifications`, `identity`, `rabbitmq`, `jaeger`
- [ ] RabbitMQ: `rabbitmq:3-management` image, AMQP port 5672, management port 15672
- [ ] RabbitMQ healthcheck: `rabbitmq-diagnostics -q ping`
- [ ] Jaeger: `jaegertracing/all-in-one` image, UI port 16686, OTLP gRPC port 4317
- [ ] All services `depends_on` RabbitMQ with `condition: service_healthy`
- [ ] Environment variables for RabbitMQ connection (`RABBITMQ__HOST`, `RABBITMQ__PORT`, etc.)
- [ ] Environment variables for OpenTelemetry (`OTEL_EXPORTER_OTLP_ENDPOINT=http://jaeger:4317`, `OTEL_SERVICE_NAME`)
- [ ] Environment variables for downstream service URLs (BFF → Products, Baskets, Orders, Identity)
- [ ] Service discovery via container names (e.g., `http://products:5001`)
- [ ] `docker-compose.override.yml` with local port bindings (5000–5005, 5672, 15672, 16686)
- [ ] `docker compose up --build` starts the entire stack successfully
- [ ] RabbitMQ management UI accessible at `http://localhost:15672` (guest/guest)
- [ ] Jaeger UI accessible at `http://localhost:16686`

---

## Saga pattern: orchestration-based checkout with compensating transactions

**What to build:** Replace the BFF's simple synchronous checkout with an orchestration-based saga that tracks state, executes steps in order, and runs compensating transactions on failure. A user can submit a checkout, see the saga progress through states (Started → BasketReserved → OrderCreated → Completed), trigger a failure (e.g., stop the Orders container), and verify the basket is restored via the compensating action. Saga state transitions are logged for visibility in `docker compose logs`.

**Blocked by:** BFF: Refit clients + routing map + checkout orchestration

- [ ] `SagaDbContext : DbContext` with `DbSet<SagaState>` using `UseInMemoryDatabase("SagaDb")`
- [ ] `SagaState` model: Id, CustomerId, CurrentState, BasketSnapshot, OrderId, CreatedAt, UpdatedAt
- [ ] `SagaState` enum: `Started`, `BasketReserved`, `OrderCreated`, `Completed`, `Compensating`, `Failed`
- [ ] Saga orchestrator in BFF: Step 1 calls Baskets checkout, stores basket snapshot for compensation
- [ ] Saga orchestrator: Step 2 calls Orders to create order, stores OrderId
- [ ] Saga orchestrator: Step 3 confirms completion, marks saga `Completed`
- [ ] Compensating action for Step 2: Baskets `POST /api/baskets/{customerId}/restore` — restores basket items from snapshot
- [ ] Compensating action for Step 3: Orders `DELETE /api/orders/{id}/cancel` — sets order status to `Cancelled`
- [ ] Compensation flow: on any step failure, run compensating actions in reverse order, mark saga `Compensating` → `Failed`
- [ ] Each saga step transition logged with saga ID and state
- [ ] Failure simulation: stopping the Orders container causes saga to compensate and restore the basket
- [ ] `GET /api/sagas/{id}` endpoint to inspect saga state (optional, for demo visibility)

---

## Distributed tracing: OpenTelemetry + Jaeger

**What to build:** Add OpenTelemetry distributed tracing to every service and a Jaeger container to Docker Compose so a user can see end-to-end traces spanning HTTP calls and RabbitMQ events. A user triggers a checkout, opens Jaeger UI at `http://localhost:16686`, and sees a single trace tree: BFF → Baskets → Orders → event publish → Notifications consume.

**Blocked by:** Scaffold solution, projects, and shared Contracts

- [ ] OpenTelemetry NuGet packages added to every service: `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.Http`, `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- [ ] Each service's `Program.cs` registers OpenTelemetry with `AddAspNetCoreInstrumentation`, `AddHttpClientInstrumentation`, `AddSource("RabbitMQ")`, `AddOtlpExporter`
- [ ] `OTEL_EXPORTER_OTLP_ENDPOINT` and `OTEL_SERVICE_NAME` environment variables configured per service
- [ ] Contracts project: `EventPublisher` injects W3C `traceparent` into RabbitMQ message headers (`IBasicProperties.Headers`)
- [ ] Contracts project: `EventConsumer` extracts `traceparent` from message headers and creates a linked activity
- [ ] Contracts project: `ActivitySource` named `"RabbitMQ"` used to create spans for publish and consume operations
- [ ] HTTP trace propagation works: BFF → downstream service calls appear as child spans in Jaeger
- [ ] RabbitMQ trace propagation works: event publish span and consume span are linked in the same trace
- [ ] Jaeger container added to `docker-compose.yml` (`jaegertracing/all-in-one`, ports 16686 + 4317)
- [ ] A checkout request produces a single trace in Jaeger spanning BFF → Baskets → Orders → event → Notifications
- [ ] Traces visible in Jaeger UI with correct service names (bff, products, baskets, orders, notifications)

---

## HTTP examples + README documentation

**What to build:** `.http` files covering all API operations (products, baskets, checkout, orders, event-driven notifications, saga failure) and a comprehensive `README.md` with one-command startup instructions, service URL table, routing map, RabbitMQ access, Jaeger access, sample request sequence, event flow explanation, saga pattern explanation, and in-memory reset limitations. A new user can follow the README from clone to verified demo.

**Blocked by:** Docker Compose: Dockerfiles + compose topology + RabbitMQ + Jaeger, Saga pattern: orchestration-based checkout with compensating transactions, Distributed tracing: OpenTelemetry + Jaeger

- [ ] `.http` file with product operations: GET all, GET by id, POST create, PUT update, DELETE
- [ ] `.http` file with basket operations: GET basket, POST add item, DELETE item
- [ ] `.http` file with checkout: POST checkout → verify order created
- [ ] `.http` file with orders: GET order by id, GET orders by customer
- [ ] `.http` file with event-driven behavior: trigger product change → check notifications
- [ ] `.http` file with saga failure: trigger checkout with Orders down → verify basket restored
- [ ] `README.md`: one-command startup (`docker compose up --build`)
- [ ] `README.md`: service URLs table (all 6 services + RabbitMQ management + Jaeger UI)
- [ ] `README.md`: routing map (BFF route → downstream service)
- [ ] `README.md`: RabbitMQ management access instructions
- [ ] `README.md`: Jaeger UI access and sample trace walkthrough
- [ ] `README.md`: step-by-step sample request sequence
- [ ] `README.md`: event flow explanation (checkout → BasketCheckedOut → OrderSubmitted → Notifications)
- [ ] `README.md`: saga pattern explanation and compensation flow
- [ ] `README.md`: distributed tracing explanation (how traces span HTTP and RabbitMQ)
- [ ] `README.md`: in-memory database reset limitations
- [ ] `README.md`: local dev without Docker (`dotnet run` + local RabbitMQ + local Jaeger)
- [ ] `README.md`: 10-step verification checklist from PLAN.md
