---
ticket: "Implementation Ticket 7"
title: "BFF: Refit clients + routing map + checkout orchestration + error handling + Swagger UI"
type: "task"
date_completed: "2026-09-10"
status: "completed"
blocked_by: ["02-products-service", "03-baskets-service", "04-identity-service", "05-orders-service"]
blocks: ["Saga pattern: orchestration-based checkout with compensating transactions", "Docker Compose: Dockerfiles + compose topology + RabbitMQ + Jaeger"]
tags: [ticket-completion]
---

# Ticket 7 — BFF: Refit Clients + Routing Map + Checkout Orchestration + Error Handling + Swagger UI

## Summary

Built the complete BFF (Backend for Frontend) service with Refit client interfaces for all four downstream services (Products, Baskets, Orders, Identity), a full routing map exposing all endpoints through the BFF on port 5000, checkout orchestration that calls Baskets to checkout and then Orders to create an order, comprehensive error handling (503 for unreachable services, 404 for not found, proper status propagation), and Swagger UI. A user can call any endpoint through the BFF on port 5000 and it routes to the correct downstream service.

## What was done

- Added NuGet packages: `Refit 8.0.0`, `Refit.HttpClientFactory 8.0.0`, `Swashbuckle.AspNetCore 10.2.3`
- Created Refit client interfaces: `IProductsClient`, `IBasketsClient`, `IOrdersClient`, `IIdentityClient` with all downstream endpoints
- Created DTOs for all services (ProductDto, BasketDto, OrderDto, CustomerDto, etc.)
- Registered Refit clients in DI with configurable base addresses from `appsettings.json` (`Downstream:ProductsUrl`, `Downstream:BasketsUrl`, `Downstream:OrdersUrl`, `Downstream:IdentityUrl`)
- Created full routing map:
  - Identity: `GET /api/identity/customer`
  - Products: `GET/POST/PUT/DELETE /api/products...`
  - Baskets: `GET/POST/DELETE /api/baskets...`
  - Orders: `GET/POST /api/orders...`
- Created checkout orchestration: `POST /api/baskets/{customerId}/checkout` → calls Baskets checkout (clears basket, returns items), then calls Orders to submit order from basket items, returns order + checked-out items
- Added error handling on all proxy endpoints: `Refit.ApiException` → propagate status code, `HttpRequestException` → 503 Service Unavailable with clean JSON problem details
- Added Swagger UI and OpenAPI specification at `/swagger`
- Updated `.http` file with example requests for all endpoints
- Set BFF port to 5000 in `launchSettings.json`
- Added `Downstream` config section to `appsettings.json` with default localhost URLs

## Key decisions

- **Refit for HTTP clients:** Used Refit 8.0.0 with `Refit.HttpClientFactory` for typed HTTP clients. Refit generates the implementation from interface attributes, reducing boilerplate. Base addresses are configurable via `appsettings.json` and environment variables (for Docker Compose).
- **Error handling with `HandleDownstreamError` helper:** A static helper method maps downstream exceptions to appropriate HTTP responses: `Refit.ApiException` propagates the downstream status code, `HttpRequestException` (service unreachable) returns 503 Service Unavailable. This prevents unhandled exceptions when downstream services are down — important for the saga ticket (Ticket 8) where failure simulation is a key demo feature.
- **Checkout orchestration is synchronous (for now):** The BFF calls Baskets to checkout, then calls Orders to create an order. This is the simple synchronous checkout — the saga pattern (Ticket 8) will replace this with orchestration-based compensation. The synchronous version is the baseline for comparison.
- **Checkout error handling:** If Baskets checkout fails → 503/propagate. If Baskets succeeds but Orders fails → 502 Bad Gateway with "manual compensation may be needed" message. This sets up the motivation for the saga pattern.
- **BFF on port 5000:** The BFF is the single entry point for the demo. Port 5000 is the standard API gateway port, easy to remember.
- **No RabbitMQ in BFF (for now):** The BFF doesn't use RabbitMQ directly — it orchestrates via HTTP calls. The saga ticket (Ticket 8) may add RabbitMQ for saga events, but the current checkout is purely HTTP-based.
- **Swashbuckle.AspNetCore for Swagger UI:** Same decision as all other services.

## Artifacts created

- `EcommerceDemo/BFF/Clients/IDownstreamClients.cs` — Refit client interfaces and all DTOs
- `EcommerceDemo/BFF/Program.cs` — Full routing map, checkout orchestration, error handling, Swagger UI
- `EcommerceDemo/BFF/BFF.csproj` — Added Refit, Refit.HttpClientFactory, Swashbuckle.AspNetCore
- `EcommerceDemo/BFF/appsettings.json` — Added Downstream URLs config section
- `EcommerceDemo/BFF/Properties/launchSettings.json` — Set port to 5000
- `EcommerceDemo/BFF/BFF.http` — Full example requests for all endpoints

## Testing & verification

- [x] `dotnet build EcommerceDemo.slnx` — Build succeeded, 0 warnings, 0 errors
- [x] BFF service starts on `http://localhost:5000`
- [x] Swagger UI accessible at `http://localhost:5000/swagger/index.html` (HTTP 200)
- [x] `GET /api/identity/customer` — Returns clean 503 JSON (Identity not running — expected)
- [x] `GET /api/products` — Returns clean 503 JSON (Products not running — expected)
- [x] Error handling works: downstream unreachable → 503, no unhandled exceptions
- [x] Refit clients correctly route to configured downstream URLs (confirmed in logs)
- [ ] Full end-to-end test (BFF → downstream services) — deferred to Docker Compose (Ticket 10)

```
dotnet build EcommerceDemo.slnx --nologo
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Dependencies

- **Blocked by:** [[02-products-service]], [[03-baskets-service]], [[04-identity-service]], [[05-orders-service]]
- **Unblocks:** Saga pattern (Ticket 8) — saga replaces the synchronous checkout
- **Unblocks:** Docker Compose (Ticket 10) — all six services now exist

## Notes for presentation

- Show the Swagger UI at `/swagger` — all endpoints from all services in one place
- Demonstrate the routing: call `GET /api/products` through the BFF, show it proxies to Products service
- Show the error handling: stop a downstream service, call the BFF endpoint, show the clean 503 response
- Demonstrate checkout orchestration: add items to basket, call checkout, show order created and basket cleared
- Point out the `Downstream` config section — shows how service URLs are configurable for Docker Compose
- The checkout error handling (502 "manual compensation may be needed") sets up the motivation for the saga pattern

## Next steps

- Saga pattern (Ticket 8) is unblocked — replaces synchronous checkout with orchestration-based compensation
- Distributed tracing (Ticket 9) is unblocked — can add OpenTelemetry to all services
- Docker Compose (Ticket 10) is unblocked — all six services now exist, ready for containerization
