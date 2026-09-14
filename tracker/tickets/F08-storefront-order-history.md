# F08 — Storefront: Order history page

**Labels:** `wayfinder:map`, `wayfinder:task`
**Parent:** [MAP — Frontend Implementation](../FRONTEND-MAP.md)
**Blocks:** F12
**Blocked by:** F04

## Question

Implement the order history page at `/shop/orders` showing all orders for `cust-001`.

### Tasks

1. **Hook** in `src/hooks/use-orders.ts`:
   - `useOrdersByCustomer(customerId)` — `useQuery` calling `getOrdersByCustomer(customerId)`, query key `['orders', customerId]`
   - `useOrder(id)` — `useQuery` calling `getOrderById(id)`, query key `['orders', id]`

2. **Page** in `src/routes/shop/orders.tsx`:
   - List of orders for `cust-001`, sorted by created date (newest first)
   - Each order card shows: order ID (truncated), status badge, total, created date
   - Click to expand: shows items (name, price, qty, line total)
   - Empty state: "No orders yet" with link to products
   - Loading state: skeleton cards
   - Error state: retry button

3. **Status badges** — Color-coded: Submitted (blue), Cancelled (red), Completed (green)

### Acceptance criteria

- Orders list renders for cust-001
- Orders sorted newest first
- Expandable order details show items and totals
- Empty state renders when no orders
- Loading and error states work
