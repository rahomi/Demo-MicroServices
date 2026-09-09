## Plan: Chapter 19 Microservices Expansion

Extend Chapter 19 into a locally runnable microservices demo using MediatR, direct RabbitMQ messaging, Docker Compose, and separate in-memory databases.

### 1. Baseline and target framework

* **SDK Verification:** Verify the installed latest stable .NET SDK. Use it when compatible; otherwise, retain .NET 8 for Chapter 19 compatibility.
* **Architecture:** Preserve the existing feature-folder and Minimal API structure.

### 2. CQRS with MediatR

* **Migration:** Migrate Products and Baskets manual handlers to `IRequest`, `IRequestHandler`, and MediatR.
* **New Services:** Add MediatR command/query handlers to the new Orders service.
* **Encapsulation:** Keep each service’s commands, queries, validators, and mappings local to that service.

### 3. Service boundaries

* **Retain existing services:**
  * BFF (Backend for Frontend)
  * Products/Catalog
  * Baskets
* **Add new services:**
  * Orders
  * Notifications
  * Lightweight Identity service using a fixed fake customer
* **Data Isolation:** Give every service its own EF Core InMemory database.

### 4. RabbitMQ integration

* **Shared Contracts:** Add a small shared contracts/infrastructure area for event DTOs and RabbitMQ connection setup.
* **Implementation Strategy:** Use direct `RabbitMQ.Client`, without MassTransit, for educational visibility.
* **Messaging Scenarios:**
  * Basket checkout messaging
  * `OrderSubmitted` integration event
  * `ProductChanged` integration event
  * Notifications consumer
* **Flow:** Keep order creation synchronous for simple local testing, while publishing integration events asynchronously.

### 5. Docker Compose

* **Container Setup:** Add Orders, Notifications, Identity, and RabbitMQ containers.
* **Configuration:** Configure service discovery and environment-based RabbitMQ settings.
* **Consistency:** Keep the existing BFF, Products, and Baskets ports and Compose conventions.

### 6. BFF and examples

* **Routing:** Add or update BFF routes for checkout and order retrieval.
* **Clients:** Update Refit clients and contracts.
* **`.http` / Postman Examples:** Add HTTP examples for:
  * Product operations
  * Basket operations
  * Checkout
  * Orders
  * Event-driven notification behavior

### 7. Documentation

* **Update `README.md` with:**
  * One-command startup instructions (`docker compose up`)
  * Service URLs and routing map
  * RabbitMQ management access (if enabled)
  * Sample request sequence
  * Event flow explanation
  * In-memory database reset limitations
  * Saga pattern explanation and compensation flow
  * Distributed tracing access (Jaeger UI URL and sample trace walkthrough)

### 8. Saga pattern (orchestration-based)

* **Checkout Saga:** Replace the simple synchronous checkout with an orchestration-based saga in the BFF.
  * **Step 1 — Reserve Basket:** Call Baskets to reserve/clear the basket.
    * *Compensating action:* Restore basket items (`POST /api/baskets/{customerId}/restore`).
  * **Step 2 — Create Order:** Call Orders to create the order.
    * *Compensating action:* Cancel order (`DELETE /api/orders/{id}/cancel`).
  * **Step 3 — Publish Event:** Publish `OrderSubmitted` integration event.
* **Saga State:** Track saga state in a small `SagaDbContext` (EF Core InMemory).
  * `SagaState` enum: `Started`, `BasketReserved`, `OrderCreated`, `Completed`, `Compensating`, `Failed`.
  * Persist each step's result so compensating actions know what to undo.
* **Compensation Flow:** If any step fails, execute compensating transactions in reverse order; mark saga `Compensating` then `Failed` (or `Completed` if compensation succeeds).
* **Educational Visibility:** Log each saga step transition so the flow is visible in `docker compose logs`.

### 9. Distributed tracing (OpenTelemetry + Jaeger)

* **OpenTelemetry SDK:** Add `OpenTelemetry.Extensions.Hosting` and tracing exporters to every service.
  * Instrument ASP.NET Core (`AddAspNetCore`), HTTP clients (`AddHttpClient`), and RabbitMQ activity sources (`AddSource("RabbitMQ")`).
  * Export traces via OTLP (`AddOtlpExporter`) to the Jaeger container.
* **HTTP Propagation:** Traces flow from BFF → downstream services via W3C `TraceContext` headers automatically.
* **RabbitMQ Span Tagging:** Tag spans when publishing and consuming events so traces cross the async messaging boundary.
* **Jaeger Container:** Add a `jaegertracing/all-in-one` container to Docker Compose (UI on port 16686, OTLP gRPC on port 4317).
* **Trace Visibility:** A user can open Jaeger UI and see the full distributed trace: BFF → Products → Baskets → Orders → event publish → Notifications consume, all in one trace tree.

---

### Relevant files

* `C19.sln` — add the new service projects.
* `Features.cs` — reuse feature registration and endpoint mapping.
* `Features.cs` — reuse for MediatR-based basket features.
* `IWebClient.cs` — extend downstream HTTP clients.
* `docker-compose.yml` — add services and RabbitMQ.
* `docker-compose.override.yml` — configure local ports and environment variables.
* `C19/C19.md` — create or update only if the repository uses this as the primary chapter documentation; otherwise use `README.md`.
* **New projects under `C19`:** `Orders`, `Notifications`, `Identity`, and likely a small shared `Contracts` project.

---

### Verification

1. Build the complete solution with the selected installed .NET SDK.
2. Start the complete stack with Docker Compose.
3. Call the BFF product and basket endpoints.
4. Submit a checkout request and verify the saga completes: basket reserved, order created, `OrderSubmitted` published.
5. Verify `OrderSubmitted` reaches the Notifications service through RabbitMQ.
6. Trigger a product change and verify `ProductChanged` is published and consumed.
7. Trigger a saga failure (e.g., Orders service down) and verify compensating actions execute (basket restored).
8. Open Jaeger UI and verify a distributed trace spans BFF → Baskets → Orders → event → Notifications.
9. Restart the containers and confirm the expected in-memory data reset behavior.
10. Confirm each service can be built and run independently where practical.

---

### Decisions

* Chapter 19 will be extended rather than duplicated.
* Direct `RabbitMQ.Client` is preferred for educational visibility over high-level abstractions.
* Authentication is deliberately excluded; Identity supplies a fake customer.
* Persistence is intentionally ephemeral and isolated per service (EF Core InMemory).
* The first deliverable is a working demonstration and README, not a production-ready platform.
* *Out of scope initially:* Kubernetes, authentication providers, durable databases, or advanced retry/outbox implementations.
* *Now in scope:* Saga pattern (orchestration-based) and distributed tracing (OpenTelemetry + Jaeger) — added to enrich the educational demo.
