# T09 — Saga orchestration and compensation design

**Labels:** `wayfinder:task`
**Parent:** [MAP — Chapter 19 Microservices Expansion](../MAP.md)
**Blocks:** T08
**Blocked by:** T04, T05

## Question

How is the checkout flow implemented as an orchestration-based saga in the BFF, including saga state persistence, step execution, and compensating transactions?

### Detail

PLAN.md §8 replaces the simple synchronous checkout with an orchestration-based saga. This ticket resolves:

1. **Saga orchestrator location** — Does the saga orchestrator live in the BFF, or in a dedicated service? (BFF is the natural home since it already orchestrates checkout.)
2. **Saga state persistence** —
   - `SagaDbContext : DbContext` with `DbSet<SagaState>` using `UseInMemoryDatabase("SagaDb")`
   - `SagaState` record: Id, CustomerId, CurrentState (enum), BasketReservationId, OrderId, CreatedAt, UpdatedAt
   - `SagaState` enum: `Started`, `BasketReserved`, `OrderCreated`, `Completed`, `Compensating`, `Failed`
3. **Step execution** —
   - Step 1: Call Baskets `POST /api/baskets/{customerId}/checkout` → store basket snapshot for compensation
   - Step 2: Call Orders `POST /api/orders` → store OrderId
   - Step 3: Publish `OrderSubmitted` event (or Orders publishes it internally)
   - Each step updates saga state in `SagaDbContext`
4. **Compensating actions** —
   - If Step 2 fails: call Baskets `POST /api/baskets/{customerId}/restore` with the basket snapshot
   - If Step 3 fails: call Orders `DELETE /api/orders/{id}/cancel`
   - Compensation runs in reverse order; saga state = `Compensating` → `Failed` (or `Completed` if all compensations succeed)
5. **Compensating endpoints** —
   - Baskets: `POST /api/baskets/{customerId}/restore` — accepts basket items, restores them
   - Orders: `DELETE /api/orders/{id}/cancel` — sets order status to `Cancelled`
6. **Failure simulation** — How does a user trigger a saga failure for testing? (e.g., a query param `?fail=true` that makes Orders return 500, or just stop the Orders container)
7. **Logging** — Each saga step transition logged with saga ID and state for `docker compose logs` visibility
8. **Timeout** — Is there a per-step timeout? Or is it fire-and-forget with the InMemory assumption that services respond quickly?

### Resolution

*(To be filled when resolved — record the saga orchestrator location, SagaState schema, step execution flow, compensating endpoint contracts, and failure simulation mechanism.)*
