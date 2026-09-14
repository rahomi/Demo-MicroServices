# F02 — API client and TypeScript types

**Labels:** `wayfinder:map`, `wayfinder:task`
**Parent:** [MAP — Frontend Implementation](../FRONTEND-MAP.md)
**Blocks:** F04
**Blocked by:** F01

## Question

Create the typed API client layer in `src/lib/api.ts` and all TypeScript types matching the backend DTOs.

### Tasks

1. **Type definitions** in `src/lib/types.ts` — Match the backend DTOs from `IDownstreamClients.cs`:
   - `Product`, `CreateProductRequest`, `UpdateProductRequest`
   - `BasketItem`, `Basket`, `AddBasketItemRequest`
   - `OrderItem`, `Order`, `SubmitOrderRequest`, `SubmitOrderItem`
   - `CheckoutResponse`, `SagaState`, `BasketItemSnapshot`
   - `Customer`
   - `NotificationEvent`

2. **API client** in `src/lib/api.ts` — Typed fetch wrapper with all endpoints:
   - `getProducts()` → `GET /api/products`
   - `getProductById(id)` → `GET /api/products/{id}`
   - `createProduct(req)` → `POST /api/products`
   - `updateProduct(id, req)` → `PUT /api/products/{id}`
   - `deleteProduct(id)` → `DELETE /api/products/{id}`
   - `getBasket(customerId)` → `GET /api/baskets/{customerId}`
   - `addBasketItem(customerId, req)` → `POST /api/baskets/{customerId}/items`
   - `removeBasketItem(customerId, productId)` → `DELETE /api/baskets/{customerId}/items/{productId}`
   - `checkout(customerId)` → `POST /api/baskets/{customerId}/checkout`
   - `getOrderById(id)` → `GET /api/orders/{id}`
   - `getOrdersByCustomer(customerId)` → `GET /api/orders?customerId={customerId}`
   - `getSagaById(id)` → `GET /api/sagas/{id}`
   - `getCustomer()` → `GET /api/identity/customer`
   - `getNotifications()` → `GET /api/notifications` (proxied to Notifications service)

3. **Error handling** — The fetch wrapper should throw on non-2xx responses with the response body as the error message, so TanStack Query's global error handler can display it.

4. **Base URL** — All requests use relative paths (`/api/...`). The Vite dev proxy and nginx handle routing.

### Acceptance criteria

- `src/lib/types.ts` with all TypeScript interfaces matching backend DTOs
- `src/lib/api.ts` with all API functions typed and working
- Fetch wrapper throws on errors with response body
- All requests use relative paths
