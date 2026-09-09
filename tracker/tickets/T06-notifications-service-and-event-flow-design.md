# T06 — Notifications service and event flow design

**Labels:** `wayfinder:task`
**Parent:** [MAP — Chapter 19 Microservices Expansion](../MAP.md)
**Blocks:** T08
**Blocked by:** T03

## Question

How does the Notifications service consume `OrderSubmitted` and `ProductChanged` events, and what is the complete event flow across all services?

### Detail

PLAN.md §4 defines messaging scenarios: Basket checkout, `OrderSubmitted`, `ProductChanged`, and a Notifications consumer. This ticket resolves:

1. **Notifications service** —
   - A minimal ASP.NET Core service that runs a `BackgroundService`/`IHostedService` consumer
   - Consumes `OrderSubmitted` → logs "Notification: Order {OrderId} submitted for {CustomerName}"
   - Consumes `ProductChanged` → logs "Notification: Product {ProductId} changed"
   - No database, no HTTP endpoints — purely a consumer? Or a `GET /api/notifications` endpoint to view received events?
2. **Event flow — Basket checkout** —
   - BFF → Baskets `POST /checkout`
   - Baskets clears the basket, publishes `BasketCheckedOut`
   - Who creates the order? BFF calls Orders directly? Or Orders consumes `BasketCheckedOut`?
   - Orders publishes `OrderSubmitted`
   - Notifications consumes `OrderSubmitted`
3. **Event flow — Product change** —
   - BFF → Products `PUT /api/products/{id}`
   - Products publishes `ProductChanged`
   - Notifications consumes `ProductChanged`
   - Does Baskets consume `ProductChanged` to update basket item prices? (PLAN.md doesn't require this but it's a natural demo.)
4. **Event visibility** — How does a user verify events reached Notifications? (Log output in `docker compose logs`? A `/api/notifications` endpoint returning received events? A simple in-memory list?)
5. **Notifications port** — What port does Notifications run on?

### Resolution

*(To be filled when resolved — record the Notifications service shape, the complete event flow diagram, and the event visibility mechanism.)*
