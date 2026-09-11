---
ticket: "3"
title: "Baskets service: MediatR CQRS + EF Core InMemory + basket ops + BasketCheckedOut event + ProductChanged consumer + Swagger UI"
type: "task"
date_completed: "2026-09-10"
status: "completed"
blocked_by: ["01-scaffold-solution"]
blocks: ["BFF: Refit clients + routing map + checkout orchestration"]
tags: [ticket-completion]
---

# Ticket 3 — Baskets Service: MediatR CQRS + EF Core InMemory + Basket Ops + BasketCheckedOut + ProductChanged Consumer + Swagger UI

## Summary

Built the complete Baskets service with MediatR command/query handlers, EF Core InMemory database, basket item add/remove endpoints, a checkout endpoint that clears the basket and publishes `BasketCheckedOut`, a `ProductChanged` event consumer that keeps basket item names/prices in sync with the Products service, and Swagger UI with OpenAPI specification. A user can create a basket, add items, remove items, and checkout — with `BasketCheckedOut` published to RabbitMQ and `ProductChanged` events consumed to update basket items.

## What was done

- Added NuGet packages: `MediatR 14.2.0`, `Microsoft.EntityFrameworkCore.InMemory 10.0.12`, `Swashbuckle.AspNetCore 10.2.3`
- Created `Basket` and `BasketItem` domain models (Basket: Id, CustomerId, Items; BasketItem: Id, BasketId, ProductId, ProductName, UnitPrice, Quantity)
- Created `BasketDbContext : DbContext` with `DbSet<Basket>` and `DbSet<BasketItem>` using `UseInMemoryDatabase("BasketsDb")`
- Created MediatR query handler: `GetBasketQuery` (by customer ID)
- Created MediatR command handlers: `AddBasketItemCommand`, `RemoveBasketItemCommand`, `CheckoutBasketCommand`
- `AddBasketItemCommand` creates the basket if it doesn't exist, increases quantity if the product is already in the basket, or adds a new line item
- `CheckoutBasketCommand` clears the basket items and publishes a `BasketCheckedOut` event via `IEventPublisher` (best-effort with graceful degradation when RabbitMQ is unavailable)
- Created `ProductChangedConsumer : EventConsumer<ProductChanged>` — a BackgroundService that consumes `product.changed` events from RabbitMQ and updates basket item names/prices (or removes items for deleted products)
- Created Minimal API endpoints: `GET /api/baskets/{customerId}`, `POST /api/baskets/{customerId}/items`, `DELETE /api/baskets/{customerId}/items/{productId}`, `POST /api/baskets/{customerId}/checkout`
- Added Swagger UI and OpenAPI specification at `/swagger` with `WithSummary`, `WithDescription`, and `Produces` metadata on each endpoint
- Updated `.http` file with example requests for all endpoints

## Key decisions

- **Explicit `BasketId` FK on `BasketItem`:** Initially used a shadow foreign key (`HasForeignKey("BasketId")`), but EF Core InMemory threw `DbUpdateConcurrencyException` when adding items to a basket after items had been removed/cleared (e.g., after checkout). Switched to an explicit `BasketId` property on `BasketItem` to avoid the change tracking issue.
- **Query `BasketItems` DbSet directly instead of `Include`:** The `Include(b => b.Items)` navigation with `basket.Items.Add()` / `basket.Items.Clear()` caused `DbUpdateConcurrencyException` in EF Core InMemory when the same basket was modified across multiple requests. All handlers now query the `BasketItems` DbSet directly for add/remove/checkout operations, only using `Include` for read-only return values.
- **Best-effort event publishing:** The `CheckoutBasketCommand` handler wraps `IEventPublisher.PublishAsync` in try-catch with a warning log, matching the Products service pattern. This allows the service to function locally without RabbitMQ running.
- **`ProductChangedConsumer` uses scoped `DbContext`:** The consumer is a singleton `IHostedService` but creates a DI scope per message to resolve `BasketDbContext` (scoped service), avoiding captive dependency issues.
- **Swashbuckle.AspNetCore for Swagger UI:** Same decision as Products service — used Swashbuckle 10.2.3 instead of `Microsoft.AspNetCore.OpenApi` to avoid deprecation warnings under `TreatWarningsAsErrors`.

## Artifacts created

- `EcommerceDemo/Baskets/Domain/Basket.cs` — Basket and BasketItem entity models
- `EcommerceDemo/Baskets/Data/BasketDbContext.cs` — EF Core InMemory DbContext
- `EcommerceDemo/Baskets/Features/Queries/GetBasket.cs` — GetBasket query handler
- `EcommerceDemo/Baskets/Features/Commands/BasketCommands.cs` — AddBasketItem, RemoveBasketItem, CheckoutBasket command handlers with BasketCheckedOut event publishing
- `EcommerceDemo/Baskets/Features/Consumers/ProductChangedConsumer.cs` — RabbitMQ consumer for ProductChanged events
- `EcommerceDemo/Baskets/Program.cs` — Updated with MediatR, EF Core, RabbitMQ, ProductChanged consumer, Swagger UI, and all Minimal API endpoints
- `EcommerceDemo/Baskets/Baskets.csproj` — Added MediatR, EF Core InMemory, Swashbuckle.AspNetCore packages
- `EcommerceDemo/Baskets/Baskets.http` — Updated with example requests for all endpoints

## Testing & verification

- [x] `dotnet build EcommerceDemo.slnx` — Build succeeded, 0 warnings, 0 errors
- [x] Service starts on `http://localhost:5201`
- [x] `GET /api/baskets/cust-001` — returns empty basket for new customer
- [x] `POST /api/baskets/cust-001/items` — adds item 1 (Wireless Mouse, qty 2)
- [x] `POST /api/baskets/cust-001/items` — adds item 2 (Mechanical Keyboard, qty 1) — both items present
- [x] `DELETE /api/baskets/cust-001/items/{productId}` — removes item 1, only item 2 remains
- [x] `POST /api/baskets/cust-001/checkout` — returns `BasketCheckedOut` event with items
- [x] Basket is empty after checkout (items cleared)
- [x] Swagger UI accessible at `http://localhost:5201/swagger/index.html` (HTTP 200)
- [x] OpenAPI spec at `/swagger/v1/swagger.json` (HTTP 200)
- [x] RabbitMQ event publishing gracefully handles RabbitMQ being unavailable (warning log, no crash)
- [x] `ProductChanged` consumer registered as BackgroundService and starts on startup

```
dotnet build EcommerceDemo.slnx --nologo
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Dependencies

- **Blocked by:** [[01-scaffold-solution]] — Scaffold solution, projects, and shared Contracts
- **Unblocks:** [[07-bff-service]] (BFF needs Baskets endpoints to proxy), [[08-saga-orchestration]] (Saga needs Baskets checkout + restore endpoints)

## Notes for presentation

- Show the Swagger UI at `/swagger` — interactive API testing for all basket operations
- Demonstrate the full basket lifecycle: add items, remove an item, checkout, verify basket is empty
- Show the service logs — RabbitMQ connection attempts and graceful degradation warnings demonstrate the messaging integration
- Point out the MediatR CQRS pattern: commands (write) and queries (read) are separated into different handlers
- The `BasketCheckedOut` event with items and timestamp shows how integration events carry the checkout payload
- The `ProductChangedConsumer` demonstrates event-driven architecture — Baskets reacts to product changes from the Products service without direct coupling
- Highlight the `ProductChanged` consumer's behavior: updates names/prices for created/updated products, removes items for deleted products

## Next steps

All downstream tickets have been completed:
- [[04-identity-service]] — Identity service (done)
- [[05-orders-service]] — Orders service (done)
- [[06-notifications-service]] — Notifications service (done)
- [[07-bff-service]] — BFF proxies Baskets endpoints (done)
- [[08-saga-orchestration]] — Saga uses Baskets checkout + restore (done)
