# F03 — TanStack Query setup with global error handling

**Labels:** `wayfinder:map`, `wayfinder:task`
**Parent:** [MAP — Frontend Implementation](../FRONTEND-MAP.md)
**Blocks:** F04
**Blocked by:** F01

## Question

Set up TanStack Query (React Query) with centralized error handling via MutationCache and QueryCache.

### Tasks

1. **Install** `@tanstack/react-query` and `sonner` (for toast notifications)

2. **QueryClient config** in `src/lib/queryClient.ts`:
   - `QueryCache` with `onError` → fire a toast: "Request failed" + error message
   - `MutationCache` with `onError` → fire a toast: "Operation failed" + error message
   - Default options: `retry: 1`, `refetchOnWindowFocus: false`
   - Wrap the `QueryClientProvider` in `App.tsx`

3. **Toaster** — Mount Sonner's `<Toaster>` in the app root with dark theme

4. **Query key conventions** — Define a query key factory pattern:
   ```typescript
   export const queryKeys = {
     products: ['products'] as const,
     product: (id: string) => ['products', id] as const,
     basket: (customerId: string) => ['basket', customerId] as const,
     orders: (customerId: string) => ['orders', customerId] as const,
     order: (id: string) => ['orders', id] as const,
     saga: (id: string) => ['saga', id] as const,
     notifications: ['notifications'] as const,
     customer: ['customer'] as const,
   }
   ```

### Acceptance criteria

- TanStack Query installed and provider wrapping the app
- Global error handlers in QueryCache and MutationCache fire toasts automatically
- Sonner toaster mounted with dark theme
- Query key factory defined
- Default retry set to 1, refetchOnWindowFocus disabled
