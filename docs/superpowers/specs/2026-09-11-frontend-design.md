# Frontend Design Spec — EcommerceDemo

**Date:** 2026-09-11  
**Status:** Approved  
**Author:** Brainstorming session

---

## Overview

A React + Vite + TypeScript frontend for the EcommerceDemo microservices project. Provides both a customer-facing storefront (browse products, basket, checkout, order history) and an admin/demo panel (product CRUD, saga state inspection, live event notifications feed). Dark theme using shadcn/ui + Tailwind CSS. Runs as a dev server during development and as a containerized nginx-served static site in Docker Compose.

---

## Tech Stack

| Technology | Purpose |
|-----------|---------|
| React 18 + TypeScript | UI framework |
| Vite | Build tool + dev server |
| React Router v6 | Routing (`/shop/*`, `/admin/*`) |
| TanStack Query (React Query) | Server state, caching, polling |
| Zustand | Client state (basket badge) |
| shadcn/ui + Radix UI | Accessible component library |
| Tailwind CSS | Styling (dark theme) |
| nginx | Static file serving + API proxy (Docker) |

---

## Project Structure

```
frontend/
├── src/
│   ├── lib/
│   │   ├── api.ts              # Typed fetch wrapper, all API endpoints
│   │   └── queryClient.ts      # TanStack Query client config
│   ├── stores/
│   │   └── basket-store.ts     # Zustand store for basket state
│   ├── hooks/
│   │   ├── use-products.ts      # useProducts, useCreateProduct, useUpdateProduct, useDeleteProduct
│   │   ├── use-basket.ts        # useBasket, useAddBasketItem, useRemoveBasketItem, useCheckout
│   │   ├── use-orders.ts        # useOrder, useOrdersByCustomer
│   │   ├── use-saga.ts          # useSagaState (with polling)
│   │   ├── use-notifications.ts  # useNotifications (with polling)
│   │   └── use-customer.ts      # useCustomer
│   ├── components/
│   │   ├── ui/                  # shadcn/ui components (Button, Card, Dialog, etc.)
│   │   └── shared/              # Layout, Navbar, Sidebar, etc.
│   ├── routes/
│   │   ├── shop/               # Storefront routes
│   │   │   ├── products.tsx     # Product grid with "Add to basket"
│   │   │   ├── basket.tsx       # Basket view with checkout button
│   │   │   ├── checkout.tsx     # Checkout confirmation + saga status
│   │   │   └── orders.tsx       # Order history
│   │   └── admin/              # Admin/Demo routes
│   │       ├── products.tsx     # Product CRUD table
│   │       ├── sagas.tsx        # Saga state inspector
│   │       └── notifications.tsx # Live notifications feed
│   ├── App.tsx                 # Router setup, layout shell
│   └── main.tsx                # Entry point
├── Dockerfile                  # Multi-stage: build → nginx serve
├── nginx.conf                  # Proxy /api → bff:5000, /api/notifications → notifications:5004
├── package.json
├── vite.config.ts             # Dev proxy: /api → localhost:5000, /api/notifications → localhost:5004
└── tailwind.config.ts
```

---

## API Endpoints Consumed

All requests go through the BFF at `http://localhost:5000` (dev) or `http://bff:5000` (Docker), except notifications which go directly to the Notifications service.

### Storefront APIs (via BFF)

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/products` | GET | Fetch all products for catalog |
| `/api/identity/customer` | GET | Get fake customer (cust-001) |
| `/api/baskets/{customerId}` | GET | Get basket contents |
| `/api/baskets/{customerId}/items` | POST | Add item to basket |
| `/api/baskets/{customerId}/items/{productId}` | DELETE | Remove item from basket |
| `/api/baskets/{customerId}/checkout` | POST | Checkout (triggers saga) |
| `/api/orders/{id}` | GET | Get order by ID |
| `/api/orders?customerId={id}` | GET | Get order history |

### Admin APIs (via BFF + Notifications service)

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/products` | POST | Create product |
| `/api/products/{id}` | PUT | Update product |
| `/api/products/{id}` | DELETE | Delete product |
| `/api/sagas/{id}` | GET | Inspect saga state |
| `/api/notifications` | GET | Get consumed events (direct to Notifications service; dev: `localhost:5004`, Docker: `notifications:5004`) |

### TypeScript Types (matching backend DTOs)

```typescript
// Products
interface Product { id: string; name: string; price: number; category: string }
interface CreateProductRequest { name: string; price: number; category: string }
interface UpdateProductRequest { id: string; name: string; price: number; category: string }

// Basket
interface BasketItem { id: string; basketId: string; productId: string; productName: string; unitPrice: number; quantity: number }
interface Basket { id: string; customerId: string; items: BasketItem[] }
interface AddBasketItemRequest { productId: string; productName: string; unitPrice: number; quantity: number }

// Orders
interface OrderItem { id: string; orderId: string; productId: string; productName: string; unitPrice: number; quantity: number }
interface Order { id: string; customerId: string; items: OrderItem[]; total: number; status: string; createdAt: string }

// Checkout / Saga
interface CheckoutResponse { sagaId: string; state: string; order: Order; message: string }
interface SagaState {
  id: string;
  customerId: string;
  currentState: string; // Started | BasketReserved | OrderCreated | Completed | Compensating | Failed
  basketSnapshotJson: string;
  orderId: string | null;
  errorMessage: string | null;
  createdAt: string;
  updatedAt: string;
}

// Identity
interface Customer { id: string; name: string; email: string }

// Notifications (direct from Notifications service)
interface NotificationEvent { type: string; data: any; receivedAt: string }
```

---

## Pages & Features

### Storefront Routes (`/shop/*`)

#### `/shop/products` — Product Catalog
- Grid of product cards showing name, price, category
- Each card has a quantity selector (default 1) and "Add to basket" button
- "Add to basket" calls `POST /api/baskets/cust-001/items` with product details
- Toast notification on success/failure
- Basket item count badge in navbar updates via Zustand

#### `/shop/basket` — Shopping Basket
- List of basket items: product name, unit price, quantity, line total
- Remove item button per row (`DELETE /api/baskets/{customerId}/items/{productId}`)
- Basket total displayed at bottom
- "Checkout" button triggers `POST /api/baskets/{customerId}/checkout`
- On success: redirect to `/shop/checkout` with saga ID + order details
- On failure (502): show error toast with compensation message, basket remains intact (items restored by saga)

#### `/shop/checkout` — Checkout Confirmation
- Shows saga state (Completed), order summary (items, total, order ID)
- Link to view order in order history
- "Back to products" button

#### `/shop/orders` — Order History
- Lists all orders for `cust-001` (`GET /api/orders?customerId=cust-001`)
- Each order expandable to show items, total, status, created date

### Admin/Demo Routes (`/admin/*`)

#### `/admin/products` — Product Management
- Table with all products (name, price, category)
- Create product (dialog with form: name, price, category)
- Edit product (dialog with pre-filled form)
- Delete product (with confirmation dialog)
- Demonstrates how product changes trigger `ProductChanged` events

#### `/admin/sagas` — Saga State Inspector
- Input field to enter a saga ID (or auto-populate from recent checkout via shared state)
- Displays saga state machine visually: Started → BasketReserved → OrderCreated → Completed (or Compensating → Failed)
- Shows basket snapshot items, order ID, error message, timestamps
- Polls every 2 seconds (`refetchInterval: 2000`) while saga is in a non-terminal state (Started, BasketReserved, OrderCreated, Compensating)
- Stops polling when state is Completed or Failed
- Includes instructions for triggering saga failure (stop orders container, checkout, observe compensation)

#### `/admin/notifications` — Event Feed
- Live feed of consumed RabbitMQ events from `GET http://localhost:5004/api/notifications`
- Auto-refreshes every 3 seconds (`refetchInterval: 3000`)
- Shows event type (BasketCheckedOut, OrderSubmitted, ProductChanged), timestamp
- Color-coded by event type
- Clear visual distinction between event types

### Shared Layout
- Top navbar with: logo, basket icon (with item count badge), nav links (Products, Basket, Orders | Admin)
- Dark theme throughout (slate/zinc palette with vibrant accents)
- Responsive (desktop-first, mobile-friendly)

---

## Data Flow

### Product → Basket → Checkout Flow
1. `useProducts()` fetches `GET /api/products` via TanStack Query (cached, stale-while-revalidate)
2. User clicks "Add to basket" → `useAddBasketItem()` mutation calls `POST /api/baskets/cust-001/items`, invalidates `basket` query on success
3. Basket badge in navbar reads from Zustand (synced with basket query data)
4. Checkout → `useCheckout()` mutation calls `POST /api/baskets/cust-001/checkout`
5. On success: navigate to `/shop/checkout` with saga ID + order, invalidate `orders` query
6. On 502 failure: toast with compensation message, basket query refetches (items restored by saga compensation)

### Saga Polling (Admin)
- `useSagaState(sagaId)` calls `GET /api/sagas/{id}` with `refetchInterval: 2000` while state is non-terminal
- Polling automatically stops when `currentState` is `Completed` or `Failed`

### Notifications Polling (Admin)
- `useNotifications()` calls `GET http://localhost:5004/api/notifications` with `refetchInterval: 3000`

---

## Error Handling

| Scenario | HTTP Status | Behavior |
|----------|-------------|----------|
| Service down | 503 | Toast: "Service is unreachable", show retry button |
| Saga failure | 502 | Toast with compensation message, basket auto-refetches to show restored items |
| Network error | — | TanStack Query retry (1 attempt), then error state with retry button |
| Not found | 404 | "Not found" state in the relevant component |
| Mutation error | 4xx/5xx | Toast with error detail from response body |

---

## Docker Integration

### docker-compose.yml addition

```yaml
frontend:
  build:
    context: frontend
    dockerfile: Dockerfile
  container_name: frontend
  depends_on:
    - bff
  ports:
    - "3000:80"
```

### nginx.conf

- Serves static React build from `/usr/share/nginx/html`
- `location /api/notifications` → `proxy_pass http://notifications:5004` (more specific, takes precedence)
- `location /api/` → `proxy_pass http://bff:5000`
- SPA fallback: `try_files $uri $uri/ /index.html`

### Dev proxy (vite.config.ts)

- `/api/notifications` → `http://localhost:5004`
- `/api` → `http://localhost:5000`

### Dockerfile (multi-stage)

```dockerfile
# Stage 1: Build
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

# Stage 2: Serve
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
```

---

## Testing

No automated test framework for this demo — it's an educational project. Manual testing via the existing 10-step verification checklist in the README, plus:

- Verify storefront: browse → add to basket → checkout → view order
- Verify admin: create/edit/delete products, inspect saga state, view notifications feed
- Verify saga failure: stop orders container, checkout, observe compensation in saga inspector and basket restoration

---

## Out of Scope (YAGNI)

- Authentication / login (Identity service returns a fixed fake customer)
- Real-time WebSocket updates (polling is sufficient for a demo)
- Pagination / infinite scroll (5 seeded products, small data set)
- Internationalization
- Automated tests
- PWA / offline support
- Product images (text-based product cards only)
