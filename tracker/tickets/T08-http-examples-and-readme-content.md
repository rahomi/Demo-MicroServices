# T08 — HTTP examples and README content

**Labels:** `wayfinder:task`
**Parent:** [MAP — Chapter 19 Microservices Expansion](../MAP.md)
**Blocks:** — (terminal ticket)
**Blocked by:** T04, T05, T06, T07

## Question

What are the `.http` / Postman examples and the README content that document the complete demo — startup, service URLs, routing map, RabbitMQ access, sample request sequence, event flow, and in-memory reset limitations?

### Detail

PLAN.md §6 and §7 require HTTP examples and README documentation. This ticket resolves:

1. **`.http` files** — Where do they live? (`C19/BFF/requests/` or per-service?) What examples are included:
   - Product operations: GET all, GET by id, POST create, PUT update, DELETE
   - Basket operations: GET basket, POST add item, DELETE item
   - Checkout: POST checkout → verify order created
   - Orders: GET order by id, GET orders by customer
   - Event-driven notification: trigger product change → check notifications
2. **README.md structure** —
   - One-command startup (`docker compose up --build`)
   - Service URLs table (BFF, Products, Baskets, Orders, Notifications, Identity, RabbitMQ Management)
   - Routing map (BFF route → downstream service)
   - RabbitMQ management access (guest/guest, `http://localhost:15672`)
   - Sample request sequence (step-by-step curl or .http walkthrough)
   - Event flow explanation (text diagram of checkout → OrderSubmitted → Notifications)
   - In-memory database reset limitations (data lost on container restart)
3. **Local dev without Docker** — How to run services individually with `dotnet run` and a local RabbitMQ instance.
4. **Verification checklist** — The 8-step verification list from PLAN.md, formatted as a runnable checklist.

### Resolution

*(To be filled when resolved — record the .http file locations, README structure, and verification checklist.)*
