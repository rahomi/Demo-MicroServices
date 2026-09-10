---
ticket: "Implementation Ticket 5"
title: "Orders service: MediatR CQRS + EF Core InMemory + order endpoints + OrderSubmitted event + Swagger UI"
type: "task"
date_completed: "2026-09-10"
status: "completed"
blocked_by: ["01-scaffold-solution"]
blocks: ["BFF: Refit clients + routing map + checkout orchestration", "Saga pattern: orchestration-based checkout with compensating transactions"]
tags: [ticket-completion]
---

# Ticket 5 — Orders Service: MediatR CQRS + EF Core InMemory + Order Endpoints + OrderSubmitted + Swagger UI

## Summary

Built the complete Orders service with MediatR command/query handlers, EF Core InMemory database, order submission (creates order, publishes `OrderSubmitted`), order query endpoints (by ID and by customer), and Swagger UI with OpenAPI specification. A user can submit an order with line items, retrieve it by ID or by customer, and see `OrderSubmitted` events published to RabbitMQ — all testable via Swagger UI at `/swagger`.

## What was done

- Added NuGet packages: `MediatR 14.2.0`, `Microsoft.EntityFrameworkCore.InMemory 10.0.12`, `Swashbuckle.AspNetCore 10.2.3`
- Created `Order` domain model (Id, CustomerId, Items, Total, Status, CreatedAt) and `OrderItem` (Id, OrderId, ProductId, ProductName, UnitPrice, Quantity)
- Created `OrderDbContext : DbContext` with `DbSet<Order>` and `DbSet<OrderItem>` using `UseInMemoryDatabase("OrdersDb")`
- Created MediatR query handlers: `GetOrderByIdQuery`, `GetOrdersByCustomerQuery`
- Created MediatR command handler: `SubmitOrderCommand` (creates order, calculates total, persists, publishes `OrderSubmitted` event via `IEventPublisher` with best-effort graceful degradation when RabbitMQ is unavailable)
- Created Minimal API endpoints: `POST /api/orders`, `GET /api/orders/{id}`, `GET /api/orders?customerId={id}`
- Added Swagger UI and OpenAPI specification at `/swagger` with `WithSummary`, `WithDescription`, and `Produces` metadata on each endpoint
- Updated `.http` file with example requests for all endpoints
- Set service port to 5202 in `launchSettings.json`

## Key decisions

- **Best-effort event publishing:** The `SubmitOrderHandler` wraps `IEventPublisher.PublishAsync` in try-catch with a warning log, matching the Products and Baskets service pattern. This allows the service to function locally without RabbitMQ running (e.g., during development), while still publishing events when RabbitMQ is available (via Docker Compose). This is intentional for the demo — not a production pattern.
- **Total calculated server-side:** The order total is computed from line items on the server (`order.Items.Sum(i => i.UnitPrice * i.Quantity)`), not trusted from the client request. This ensures data integrity.
- **Explicit FK on OrderItem:** `OrderItem` has an explicit `OrderId` foreign key property mapped via `HasMany(o => o.Items).WithOne().HasForeignKey(i => i.OrderId)`, following the same explicit-FK pattern used in the Baskets service to avoid EF Core InMemory change tracking issues.
- **Swashbuckle.AspNetCore for Swagger UI:** Same decision as Products, Baskets, and Identity services — used Swashbuckle 10.2.3 for consistency and to avoid deprecation warnings under `TreatWarningsAsErrors`.
- **No seed data:** Unlike Products (which seeds 5 products), the Orders service starts with an empty database. Orders are created via the `POST /api/orders` endpoint. This is intentional — orders are user-generated, not pre-seeded.

## Artifacts created

- `EcommerceDemo/Orders/Domain/Order.cs` — Order and OrderItem entity models
- `EcommerceDemo/Orders/Data/OrderDbContext.cs` — EF Core InMemory DbContext
- `EcommerceDemo/Orders/Features/Queries/GetOrders.cs` — GetOrderById and GetOrdersByCustomer query handlers
- `EcommerceDemo/Orders/Features/Commands/SubmitOrder.cs` — SubmitOrder command handler with OrderSubmitted event publishing
- `EcommerceDemo/Orders/Program.cs` — Updated with MediatR, EF Core, RabbitMQ, Swagger UI, and all Minimal API endpoints
- `EcommerceDemo/Orders/Orders.csproj` — Added MediatR, EF Core InMemory, Swashbuckle.AspNetCore packages
- `EcommerceDemo/Orders/Properties/launchSettings.json` — Set port to 5202
- `EcommerceDemo/Orders/Orders.http` — Updated with example requests for all endpoints

## Testing & verification

- [x] `dotnet build EcommerceDemo.slnx` — Build succeeded, 0 warnings, 0 errors
- [x] Service starts on `http://localhost:5202`
- [x] `POST /api/orders` — Creates order with 2 items (Wireless Mouse x2, Mechanical Keyboard x1), total = 149.97, status = "Submitted", returns 201 Created
- [x] `GET /api/orders/{id}` — Returns the created order with line items (200 OK)
- [x] `GET /api/orders?customerId=cust-001` — Returns list of orders for customer (200 OK)
- [x] Swagger UI accessible at `http://localhost:5202/swagger/index.html` (HTTP 200)
- [x] RabbitMQ event publishing gracefully handles RabbitMQ being unavailable (warning log, no crash)

```
dotnet build EcommerceDemo.slnx --nologo
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Dependencies

- **Blocked by:** [[01-scaffold-solution]] — Scaffold solution, projects, and shared Contracts
- **Unblocks:** BFF: Refit clients + routing map + checkout orchestration (BFF needs Orders endpoints to proxy)
- **Unblocks:** Saga pattern: orchestration-based checkout (saga needs Orders to create orders and cancel them)

## Notes for presentation

- Show the Swagger UI at `/swagger` — interactive API testing for all order operations
- Demonstrate the order lifecycle: submit an order with multiple line items, retrieve it by ID, list orders by customer
- Point out the MediatR CQRS pattern: commands (write) and queries (read) are separated into different handlers
- The `OrderSubmitted` event with items, total, and timestamp shows how integration events carry the full order payload
- Show the service logs — RabbitMQ connection attempts and graceful degradation warnings demonstrate the messaging integration
- The order total is calculated server-side from line items, not trusted from the client — a good data integrity talking point

## Next steps

- Notifications service (Ticket 6) is unblocked — can consume `OrderSubmitted` events
- BFF (Ticket 7) needs Orders endpoints to proxy — now available
- Saga pattern (Ticket 8) needs Orders to create and cancel orders — now available
