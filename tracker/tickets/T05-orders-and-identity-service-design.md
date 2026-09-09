# T05 — Orders and Identity service design

**Labels:** `wayfinder:task`
**Parent:** [MAP — Chapter 19 Microservices Expansion](../MAP.md)
**Blocks:** T08
**Blocked by:** T01, T03

## Question

What are the domain models, EF Core InMemory configurations, MediatR handlers, and event publishing/consuming responsibilities for the new Orders and Identity services?

### Detail

PLAN.md §3 adds Orders, Notifications, and Identity services. This ticket covers Orders and Identity (Notifications is T06). It resolves:

1. **Orders domain model** —
   - `Order` (Id, CustomerId, Items, Total, Status, CreatedAt)
   - `OrderItem` (ProductId, ProductName, UnitPrice, Quantity)
   - `OrderStatus` enum (Pending, Submitted, Confirmed, Shipped)
2. **Orders EF Core InMemory** — `OrderDbContext : DbContext` with `DbSet<Order>`. How is the DB named/seeded? (`UseInMemoryDatabase("OrdersDb")`)
3. **Orders MediatR handlers** —
   - `SubmitOrder` command (creates order, publishes `OrderSubmitted` event, returns order)
   - `GetOrderById` query
   - `GetOrdersByCustomer` query
4. **Orders event consumption** — Does Orders consume `BasketCheckedOut`? Or does BFF call Orders directly on checkout? (Depends on T04's checkout flow decision.)
5. **Orders event publishing** — Orders publishes `OrderSubmitted` on order creation.
6. **Identity service** —
   - Returns a fixed fake customer: `{ Id: "cust-001", Name: "Test Customer", Email: "test@demo.local" }`
   - Single endpoint: `GET /api/customer`
   - No database, no MediatR — just a static response? Or minimal MediatR for consistency?
7. **Identity port** — What port does Identity run on?

### Resolution

*(To be filled when resolved — record the Orders domain model, DbContext, handler list, event publish/consume responsibilities, and Identity service shape.)*
