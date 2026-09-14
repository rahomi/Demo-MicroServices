# F04 — Shared layout, navbar, and React Router setup

**Labels:** `wayfinder:map`, `wayfinder:task`
**Parent:** [MAP — Frontend Implementation](../FRONTEND-MAP.md)
**Blocks:** F05, F06, F07, F08, F09, F10, F11, F12
**Blocked by:** F02, F03

## Question

Create the shared layout shell, navbar with basket badge, and React Router route configuration.

### Tasks

1. **Layout component** in `src/components/shared/Layout.tsx`:
   - Dark theme container
   - Top navbar with: logo, nav links (Products, Basket, Orders | Admin), basket icon with item count badge
   - `<Outlet />` for page content

2. **Navbar** in `src/components/shared/Navbar.tsx`:
   - Logo on the left
   - Nav links: Products (`/shop/products`), Basket (`/shop/basket`), Orders (`/shop/orders`)
   - Admin link: `/admin/products` (or dropdown with Products, Sagas, Notifications)
   - Basket icon with badge showing total item count from `useBasket('cust-001')` (TanStack Query cache — no Zustand)
   - Active link styling

3. **React Router setup** in `src/App.tsx`:
   - `BrowserRouter` with routes:
     - `/` → redirect to `/shop/products`
     - `/shop/products` → Product catalog (F05)
     - `/shop/basket` → Basket page (F06)
     - `/shop/checkout` → Checkout confirmation (F07)
     - `/shop/orders` → Order history (F08)
     - `/admin/products` → Product management (F09)
     - `/admin/sagas` → Saga inspector (F10)
     - `/admin/notifications` → Notifications feed (F11)
   - All routes wrapped in the shared Layout

4. **Customer ID constant** — Define `CUSTOMER_ID = 'cust-001'` in a constants file, used across all hooks and pages.

5. **Placeholder pages** — Create stub components for each route that just render a heading, so routing works before page implementations.

### Acceptance criteria

- Layout with dark theme navbar renders on all routes
- Basket badge in navbar reads from `useBasket()` via TanStack Query cache
- All routes defined and navigable
- `/` redirects to `/shop/products`
- Placeholder pages render for each route
- Customer ID constant defined
