# F11 — Admin: Live notifications feed page

**Labels:** `wayfinder:map`, `wayfinder:task`
**Parent:** [MAP — Frontend Implementation](../FRONTEND-MAP.md)
**Blocks:** F12
**Blocked by:** F04

## Question

Implement the live notifications feed page at `/admin/notifications` showing consumed RabbitMQ events.

### Tasks

1. **Hook** in `src/hooks/use-notifications.ts`:
   - `useNotifications()` — `useQuery` calling `getNotifications()`, query key `['notifications']`
   - `refetchInterval: 3000` (auto-refresh every 3 seconds)
   - **Client-side cap:** Slice response to latest 50 events before returning (use `select` option in useQuery)
   - **Deduplication:** Deduplicate by event type + timestamp to prevent flicker on re-fetch (use `select` option)

2. **Page** in `src/routes/admin/notifications.tsx`:
   - Feed of notification events, newest first
   - Each event card shows:
     - Event type badge (color-coded)
     - Timestamp (formatted, relative time like "2s ago")
     - Event data (JSON, collapsible)
   - Color-coded by event type:
     - `BasketCheckedOut`: cyan
     - `OrderSubmitted`: green
     - `ProductChanged`: amber
   - Auto-refresh indicator (pulsing dot or "Live" badge)
   - Empty state: "No events yet. Try adding items to basket and checking out, or updating a product."
   - Loading state: skeleton cards
   - Error state: retry button

3. **Event data display** — Pretty-print JSON in a `<pre>` block with monospace font, collapsible per event.

### Acceptance criteria

- Notifications feed renders with auto-refresh every 3s
- Events sorted newest first
- Color-coded by event type
- Only latest 50 events shown (client-side cap)
- Deduplication prevents flicker
- Event data displayed as formatted JSON
- Empty and error states render correctly
