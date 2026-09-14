# MAP — Frontend Implementation

**Labels:** `wayfinder:map`

## Destination

A React + Vite + TypeScript frontend for the EcommerceDemo microservices project, with a storefront (product catalog, basket, checkout, order history) and an admin/demo panel (product CRUD, saga state inspector, live notifications feed). Dark theme using shadcn/ui + Tailwind CSS. Runs as a dev server and as a containerized nginx-served static site in Docker Compose on port 3000.

## Notes

- **Domain:** React 18, Vite, TypeScript, TanStack Query, shadcn/ui + Radix UI, Tailwind CSS, React Router v6, nginx.
- **Source of truth:** `docs/superpowers/specs/2026-09-11-frontend-design.md`
- **Standing preferences:**
  - TanStack Query is the single source of truth for server state — no Zustand for server data.
  - Global error handling via MutationCache/QueryCache — toasts fire automatically.
  - Dark theme throughout (slate/zinc palette with vibrant accents).
  - Notifications endpoint goes directly to the Notifications service (port 5004), not through the BFF.
  - No automated tests — educational demo, manual verification via the 10-step checklist.

## Decisions so far

- **Spec approved:** Full design spec at `docs/superpowers/specs/2026-09-11-frontend-design.md` — covers tech stack, project structure, API endpoints, TypeScript types, pages, data flow, error handling, and Docker integration.

## Ticket index

| Ticket | Title | Type | Blocked by | Blocks |
|--------|-------|------|------------|--------|
| [F01](tickets/F01-scaffold-vite-react-project.md) | Scaffold Vite + React + TypeScript project with Tailwind and shadcn/ui | task | — | F02, F03, F04 |
| [F02](tickets/F02-api-client-and-types.md) | API client and TypeScript types | task | F01 | F04 |
| [F03](tickets/F03-tanstack-query-and-error-handling.md) | TanStack Query setup with global error handling | task | F01 | F04 |
| [F04](tickets/F04-shared-layout-and-routing.md) | Shared layout, navbar, and React Router setup | task | F02, F03 | F05, F06, F07, F08, F09, F10, F11, F12 |
| [F05](tickets/F05-storefront-product-catalog.md) | Storefront: Product catalog page | task | F04 | F12 |
| [F06](tickets/F06-storefront-basket-page.md) | Storefront: Basket page with checkout | task | F04 | F07, F12 |
| [F07](tickets/F07-storefront-checkout-confirmation.md) | Storefront: Checkout confirmation page | task | F06 | F12 |
| [F08](tickets/F08-storefront-order-history.md) | Storefront: Order history page | task | F04 | F12 |
| [F09](tickets/F09-admin-product-management.md) | Admin: Product management CRUD page | task | F04 | F12 |
| [F10](tickets/F10-admin-saga-inspector.md) | Admin: Saga state inspector page | task | F04 | F12 |
| [F11](tickets/F11-admin-notifications-feed.md) | Admin: Live notifications feed page | task | F04 | F12 |
| [F12](tickets/F12-docker-integration.md) | Docker integration: Dockerfile, nginx.conf, docker-compose | task | F04 | — |

### Frontier (open, unblocked, unclaimed)

- **F01** — Scaffold Vite + React + TypeScript project with Tailwind and shadcn/ui

### Blocked (open, waiting on dependencies)

- F02 ← F01
- F03 ← F01
- F04 ← F02, F03
- F05 ← F04
- F06 ← F04
- F07 ← F06
- F08 ← F04
- F09 ← F04
- F10 ← F04
- F11 ← F04
- F12 ← F04

## Not yet specified

_(None — the spec is comprehensive and the way is clear.)_

## Out of scope

- **Authentication / login** — Identity service returns a fixed fake customer.
- **Real-time WebSocket updates** — Polling is sufficient for a demo.
- **Pagination / infinite scroll** — 5 seeded products, small data set.
- **Internationalization** — English only.
- **Automated tests** — Educational project, manual verification.
- **PWA / offline support** — Not needed for a demo.
- **Product images** — Text-based product cards only.
