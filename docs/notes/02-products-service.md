---
ticket: "Implementation Ticket 2"
title: "Products service: MediatR CQRS + EF Core InMemory + CRUD + ProductChanged event + Swagger UI"
type: "task"
date_completed: "2026-09-10"
status: "completed"
blocked_by: ["01-scaffold-solution"]
blocks: ["BFF: Refit clients + routing map + checkout orchestration"]
tags: [ticket-completion]
---

# Ticket 2 — Products Service: MediatR CQRS + EF Core InMemory + CRUD + ProductChanged + Swagger UI

## Summary

Built the complete Products/Catalog service with MediatR command/query handlers, EF Core InMemory database with seed data, full CRUD endpoints via Minimal API, `ProductChanged` event publishing on create/update/delete, and Swagger UI with OpenAPI specification for interactive API testing. A user can start the service, browse seeded products, create/update/delete products, and see `ProductChanged` events published to RabbitMQ — all testable via Swagger UI at `/swagger`.

## What was done

- Added NuGet packages: `MediatR 14.2.0`, `Microsoft.EntityFrameworkCore.InMemory 10.0.12`, `Swashbuckle.AspNetCore 10.2.3`
- Created `Product` domain model (Id, Name, Price, Category)
- Created `ProductDbContext : DbContext` with `DbSet<Product>` using `UseInMemoryDatabase("ProductsDb")`
- Created `ProductDbSeeder` — seeds 5 sample products on startup (Wireless Mouse, Mechanical Keyboard, USB-C Hub, 27-inch Monitor, Laptop Stand)
- Created MediatR query handlers: `GetProductsQuery`, `GetProductByIdQuery`
- Created MediatR command handlers: `CreateProductCommand`, `UpdateProductCommand`, `DeleteProductCommand`
- Each command handler publishes a `ProductChanged` event to RabbitMQ via `IEventPublisher` (best-effort with graceful degradation when RabbitMQ is unavailable)
- Created Minimal API endpoints: `GET /api/products`, `GET /api/products/{id}`, `POST /api/products`, `PUT /api/products/{id}`, `DELETE /api/products/{id}`
- Added Swagger UI and OpenAPI specification at `/swagger` with `WithSummary`, `WithDescription`, and `Produces` metadata on each endpoint
- Updated `.http` file with example requests for all endpoints

## Key decisions

- **Best-effort event publishing:** Command handlers wrap `IEventPublisher.PublishAsync` in try-catch with warning log. This allows the service to function locally without RabbitMQ running (e.g., during development), while still publishing events when RabbitMQ is available (via Docker Compose). This is intentional for the demo — not a production pattern.
- **Swashbuckle.AspNetCore for Swagger UI:** Used Swashbuckle 10.2.3 instead of `Microsoft.AspNetCore.OpenApi` because the latter's `WithOpenApi()` extension is deprecated in .NET 10 (fails under `TreatWarningsAsErrors`). Swashbuckle provides `AddSwaggerGen`, `UseSwagger`, and `UseSwaggerUI` without deprecation warnings.
- **No `GenerateDocumentationFile`:** XML doc comments in top-level `Program.cs` cause CS1587 errors when `GenerateDocumentationFile` is enabled. Used `WithSummary()`/`WithDescription()` instead for Swagger metadata.
- **`Microsoft.OpenApi` namespace:** In Microsoft.OpenApi 2.x (used by Swashbuckle 10.x), `OpenApiInfo` is in the `Microsoft.OpenApi` root namespace, not `Microsoft.OpenApi.Models`.

## Artifacts created

- `EcommerceDemo/Products/Domain/Product.cs` — Product entity model
- `EcommerceDemo/Products/Data/ProductDbContext.cs` — EF Core InMemory DbContext
- `EcommerceDemo/Products/Data/ProductDbSeeder.cs` — Seeds 5 sample products on startup
- `EcommerceDemo/Products/Features/Queries/GetProducts.cs` — GetProducts and GetProductById query handlers
- `EcommerceDemo/Products/Features/Commands/CreateProduct.cs` — Create, Update, Delete command handlers with ProductChanged event publishing
- `EcommerceDemo/Products/Program.cs` — Updated with MediatR, EF Core, RabbitMQ, Swagger UI, and all Minimal API endpoints
- `EcommerceDemo/Products/Products.csproj` — Added MediatR, EF Core InMemory, Swashbuckle.AspNetCore packages
- `EcommerceDemo/Products/Products.http` — Updated with example requests for all endpoints

## Testing & verification

- [x] `dotnet build EcommerceDemo.slnx` — Build succeeded, 0 warnings, 0 errors
- [x] Service starts on `http://localhost:5024` and seeds 5 products
- [x] `GET /api/products` — returns all 5 seeded products
- [x] `GET /api/products/{id}` — returns single product by ID
- [x] `POST /api/products` — creates product, returns 201 Created
- [x] `PUT /api/products/{id}` — updates product, returns updated product
- [x] `DELETE /api/products/{id}` — returns 204 No Content
- [x] Swagger UI accessible at `http://localhost:5024/swagger/index.html` (HTTP 200)
- [x] OpenAPI spec at `/swagger/v1/swagger.json` contains all endpoints with proper title and paths
- [x] RabbitMQ event publishing gracefully handles RabbitMQ being unavailable (warning log, no crash)

```
dotnet build EcommerceDemo.slnx --nologo
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Dependencies

- **Blocked by:** [[01-scaffold-solution]] — Scaffold solution, projects, and shared Contracts
- **Unblocks:** BFF: Refit clients + routing map + checkout orchestration (BFF needs Products endpoints to proxy)

## Notes for presentation

- Show the Swagger UI at `/swagger` — it's the most visual and interactive way to demonstrate the API
- Demonstrate the full CRUD lifecycle: create a product, get it by ID, update it, delete it
- Show the service logs — RabbitMQ connection attempts and graceful degradation warnings demonstrate the messaging integration
- The 5 seeded products make the demo instantly usable without any setup
- Point out the MediatR CQRS pattern: commands (write) and queries (read) are separated into different handlers
- The `ProductChanged` event with `ChangeType` ("created", "updated", "deleted") shows how integration events carry context about what happened

## Next steps

- Baskets service (Ticket 3) is unblocked — it consumes `ProductChanged` events
- Identity service (Ticket 4) is unblocked — independent of Products
- Orders service (Ticket 5) is unblocked — independent of Products
- BFF (Ticket 7) needs Products endpoints to proxy — now available
