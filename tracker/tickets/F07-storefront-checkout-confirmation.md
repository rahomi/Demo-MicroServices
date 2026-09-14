# F07 — Storefront: Checkout confirmation page

**Labels:** `wayfinder:map`, `wayfinder:task`
**Parent:** [MAP — Frontend Implementation](../FRONTEND-MAP.md)
**Blocks:** F12
**Blocked by:** F06

## Question

Implement the checkout confirmation page at `/shop/checkout` that displays the saga result and order summary.

### Tasks

1. **Page** in `src/routes/shop/checkout.tsx`:
   - Read saga ID + order data from router state (passed by F06 basket page on successful checkout)
   - If no data (direct navigation), show "No recent checkout" with link to basket
   - Display:
     - Saga state badge: "Completed" (green)
     - Saga ID (monospace)
     - Order summary: order ID, items (name, price, qty, line total), order total, status, created date
     - "View order in history" link → `/shop/orders`
     - "Back to products" button → `/shop/products`

2. **Edge cases**:
   - Direct navigation without router state → friendly empty state
   - Order data may be null if saga failed → show failure state with link back to basket

### Acceptance criteria

- Displays saga state, saga ID, and order summary after successful checkout
- "View order in history" navigates to `/shop/orders`
- "Back to products" navigates to `/shop/products`
- Direct navigation shows empty state
