---
ticket: "4"
title: "Identity service: fake customer endpoint + Swagger UI"
type: "task"
date_completed: "2026-09-10"
status: "completed"
blocked_by: ["01-scaffold-solution"]
blocks: ["BFF: Refit clients + routing map + checkout orchestration"]
tags: [ticket-completion]
---

# Ticket 4 — Identity Service: Fake Customer Endpoint + Swagger UI

## Summary

Built the minimal Identity service with a single `GET /api/customer` endpoint returning a fixed fake customer (`cust-001`, `Test Customer`, `test@demo.local`). No database, no RabbitMQ, no MediatR — intentionally minimal. Added Swagger UI for consistency with the other services. A user can call `GET /api/customer` and receive the fake customer JSON.

## What was done

- Added NuGet package: `Swashbuckle.AspNetCore 10.2.3`
- Created `GET /api/customer` endpoint returning a fixed anonymous object: `{ Id: "cust-001", Name: "Test Customer", Email: "test@demo.local" }`
- Added Swagger UI and OpenAPI specification at `/swagger` with `WithSummary`, `WithDescription`, and `Produces` metadata
- Updated `.http` file with example request for the endpoint
- No database, no RabbitMQ, no MediatR — intentionally minimal per the ticket requirements

## Key decisions

- **Anonymous object for the response:** The fake customer is returned as an anonymous object directly from the endpoint. No domain model or DTO is needed for a single static response — keeping the service intentionally minimal.
- **Swashbuckle.AspNetCore for Swagger UI:** Same decision as Products and Baskets services — used Swashbuckle 10.2.3 for consistency and to avoid deprecation warnings under `TreatWarningsAsErrors`.
- **No Contracts reference needed:** Although the project references Contracts (from the scaffold), the Identity service doesn't use any RabbitMQ messaging or event DTOs. The reference is retained but unused.

## Artifacts created

- `EcommerceDemo/Identity/Program.cs` — Updated with Swagger UI and `GET /api/customer` endpoint
- `EcommerceDemo/Identity/Identity.csproj` — Added Swashbuckle.AspNetCore package
- `EcommerceDemo/Identity/Identity.http` — Updated with example request

## Testing & verification

- [x] `dotnet build EcommerceDemo.slnx` — Build succeeded, 0 warnings, 0 errors
- [x] Service starts on `http://localhost:5003`
- [x] `GET /api/customer` — returns `{ "id": "cust-001", "name": "Test Customer", "email": "test@demo.local" }`
- [x] Swagger UI accessible at `http://localhost:5003/swagger/index.html` (HTTP 200)
- [x] No database, no RabbitMQ, no MediatR — intentionally minimal

```
dotnet build EcommerceDemo.slnx --nologo
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Dependencies

- **Blocked by:** [[01-scaffold-solution]] — Scaffold solution, projects, and shared Contracts
- **Unblocks:** [[07-bff-service]] (BFF needs Identity endpoint to proxy)

## Notes for presentation

- This is the simplest service in the demo — show it to explain the service boundary concept
- Point out that authentication is deliberately excluded; Identity supplies a fake customer
- The single endpoint demonstrates that not every microservice needs a database or messaging infrastructure
- Swagger UI at `/swagger` provides interactive testing

## Next steps

All downstream tickets have been completed:
- [[05-orders-service]] — Orders service (done)
- [[06-notifications-service]] — Notifications service (done)
- [[07-bff-service]] — BFF proxies Identity endpoint (done)
