# F09 — Admin: Product management CRUD page

**Labels:** `wayfinder:map`, `wayfinder:task`
**Parent:** [MAP — Frontend Implementation](../FRONTEND-MAP.md)
**Blocks:** F12
**Blocked by:** F04

## Question

Implement the admin product management page at `/admin/products` with full CRUD operations.

### Tasks

1. **Hooks** — Reuse `useProducts`, `useCreateProduct`, `useUpdateProduct`, `useDeleteProduct` from `src/hooks/use-products.ts` (created in F05).

2. **Page** in `src/routes/admin/products.tsx`:
   - Table with columns: Name, Price, Category, Actions (edit, delete)
   - "Create Product" button at top → opens dialog with form (name, price, category)
   - Edit button per row → opens dialog pre-filled with product data
   - Delete button per row → confirmation dialog, then deletes
   - Loading state: table skeleton
   - Error state: retry button

3. **Create/Edit dialog** — shadcn/ui Dialog with form:
   - Name (text input, required)
   - Price (number input, required, min 0)
   - Category (text input, required)
   - Save / Cancel buttons
   - Form validation (required fields, numeric price)

4. **Delete confirmation** — shadcn/ui AlertDialog:
   - "Are you sure you want to delete {name}?"
   - Confirm / Cancel

5. **Note** — This page demonstrates how product changes trigger `ProductChanged` events (visible in the notifications feed at `/admin/notifications`).

### Acceptance criteria

- Product table renders all products
- Create product dialog works with validation
- Edit product dialog pre-fills and updates
- Delete with confirmation works
- All mutations invalidate the products query and update the table
- Toast notifications fire on success/failure via global handler
