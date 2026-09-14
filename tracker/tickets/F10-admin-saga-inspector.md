# F10 — Admin: Saga state inspector page

**Labels:** `wayfinder:map`, `wayfinder:task`
**Parent:** [MAP — Frontend Implementation](../FRONTEND-MAP.md)
**Blocks:** F12
**Blocked by:** F04

## Question

Implement the saga state inspector page at `/admin/sagas` with visual state machine and polling.

### Tasks

1. **Hook** in `src/hooks/use-saga.ts`:
   - `useSagaState(sagaId)` — `useQuery` calling `getSagaById(sagaId)`, query key `['saga', sagaId]`
   - `refetchInterval: 2000` while `currentState` is non-terminal (Started, BasketReserved, OrderCreated, Compensating)
   - Polling stops automatically when state is Completed or Failed (set `refetchInterval` to `false`)
   - `enabled: !!sagaId` — only fetches when a saga ID is provided

2. **Page** in `src/routes/admin/sagas.tsx`:
   - Input field for saga ID (with "Load" button or auto-search on enter)
   - Auto-populate from recent checkout if available (shared state from F06 — e.g., localStorage or query param)
   - Visual state machine: horizontal flow showing states with the current one highlighted:
     - Started → BasketReserved → OrderCreated → Completed (green path)
     - Compensating → Failed (red path, shown when applicable)
   - Detail panel showing:
     - Saga ID, customer ID
     - Current state (badge, color-coded)
     - Basket snapshot items (product name, price, qty) — parsed from `basketSnapshotJson`
     - Order ID (if created, with link to order)
     - Error message (if failed)
     - Created at, updated at timestamps
   - Empty state: "Enter a saga ID to inspect"

3. **Saga failure instructions** — Info box on the page:
   - "To test saga compensation: 1) Add items to basket, 2) Stop the orders container (`docker compose stop orders`), 3) Checkout from the basket page, 4) Return here and enter the saga ID to see the Failed state with compensation details."

4. **State colors**:
   - Started: blue
   - BasketReserved: cyan
   - OrderCreated: amber
   - Completed: green
   - Compensating: orange
   - Failed: red

### Acceptance criteria

- Saga ID input loads saga state
- Visual state machine highlights current state
- Polling every 2s while non-terminal, stops on Completed/Failed
- Basket snapshot parsed and displayed
- Error message shown for failed sagas
- Timestamps displayed
- Failure instructions visible
