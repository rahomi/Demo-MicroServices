# F06 — Storefront: Basket page with checkout

**Labels:** `wayfinder:map`, `wayfinder:task`
**Parent:** [MAP — Frontend Implementation](../FRONTEND-MAP.md)
**Blocks:** F07, F12
**Blocked by:** F04

## Question

Implement the basket page at `/shop/basket` with item list, remove, total, and checkout button.

### Tasks

1. **Hooks** in `src/hooks/use-basket.ts`:
   - `useBasket(customerId)` — `useQuery` calling `getBasket(customerId)`, query key `['basket', customerId]`
   - `useRemoveBasketItem(customerId)` — `useMutation` calling `removeBasketItem()`, invalidates `['basket', customerId]`
   - `useCheckout(customerId)` — `useMutation` calling `checkout(customerId)`:
     - `onSuccess`: navigate to `/shop/checkout` with saga ID + order data (pass via router state or query params)
     - `onError` (502): show compensation toast with error message, invalidate basket query (items restored by saga)
     - Invalidate `['orders', customerId]` on success

2. **Page** in `src/routes/shop/basket.tsx`:
   - Table/list of basket items: product name, unit price, quantity, line total
   - Remove button per row (trash icon)
   - Basket total at bottom (sum of unitPrice × quantity)
   - "Checkout" button — calls `useCheckout`, shows loading state during saga
   - Empty basket state: "Your basket is empty" with link to products
   - Loading state: skeleton rows
   - Error state: retry button

3. **Checkout error handling** — The 502 response from the BFF contains the compensation message. Display it in a toast: "Checkout saga failed — basket restored". The basket query refetches automatically (invalidated), showing restored items.

### Acceptance criteria

- Basket items render with name, price, quantity, line total
- Remove item works and updates the list
- Basket total calculates correctly
- Checkout navigates to `/shop/checkout` on success with saga ID + order
- Checkout failure (502) shows compensation toast, basket shows restored items
- Empty basket state renders
