# F05 — Storefront: Product catalog page

**Labels:** `wayfinder:map`, `wayfinder:task`
**Parent:** [MAP — Frontend Implementation](../FRONTEND-MAP.md)
**Blocks:** F12
**Blocked by:** F04

## Question

Implement the product catalog page at `/shop/products`.

### Tasks

1. **Hook** in `src/hooks/use-products.ts`:
   - `useProducts()` — `useQuery` calling `getProducts()`, query key `['products']`
   - `useCreateProduct()` — `useMutation` calling `createProduct()`, invalidates `['products']`
   - `useUpdateProduct()` — `useMutation` calling `updateProduct()`, invalidates `['products']`
   - `useDeleteProduct()` — `useMutation` calling `deleteProduct()`, invalidates `['products']`

2. **Page** in `src/routes/shop/products.tsx`:
   - Grid of product cards (name, price, category)
   - Each card has a quantity selector (default 1) and "Add to basket" button
   - "Add to basket" calls `addBasketItem(CUSTOMER_ID, { productId, productName, unitPrice, quantity })`
   - Success toast: "Added to basket" (fires via global handler or explicit onSuccess)
   - Loading state: skeleton cards or spinner
   - Error state: retry button
   - Empty state: "No products available"

3. **Add to basket hook** — `useAddBasketItem()` in `src/hooks/use-basket.ts`:
   - `useMutation` calling `addBasketItem(customerId, req)`
   - `onSuccess`: invalidate `['basket', customerId]` query
   - Toast fires automatically via global MutationCache

### Acceptance criteria

- Product grid renders with all seeded products
- Quantity selector works (1-10 range)
- "Add to basket" calls the API and shows success toast
- Basket badge in navbar updates after adding
- Loading and error states render correctly
