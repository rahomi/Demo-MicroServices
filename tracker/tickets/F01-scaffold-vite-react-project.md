# F01 — Scaffold Vite + React + TypeScript project with Tailwind and shadcn/ui

**Labels:** `wayfinder:map`, `wayfinder:task`
**Parent:** [MAP — Frontend Implementation](../FRONTEND-MAP.md)
**Blocks:** F02, F03, F04
**Blocked by:** — (frontier)

## Question

Scaffold the frontend project at `frontend/` in the repo root with the following setup:

1. **Vite + React + TypeScript** — `npm create vite@latest frontend -- --template react-ts`
2. **Tailwind CSS** — Install and configure with dark theme defaults (slate/zinc palette)
3. **shadcn/ui** — Initialize with dark theme, add base components: Button, Card, Dialog, Input, Label, Table, Badge, Toast/Sonner, Select
4. **React Router v6** — Install `react-router-dom`
5. **Path aliases** — Configure `@/` pointing to `src/` in `tsconfig.json` and `vite.config.ts`
6. **Vite dev proxy** — Configure `vite.config.ts` to proxy:
   - `/api/notifications` → `http://localhost:5004`
   - `/api` → `http://localhost:5000`
7. **Dark theme** — Set up Tailwind with `dark` class on `<html>` by default
8. **Verify** — `npm run dev` starts without errors, `npm run build` produces a `dist/` folder

### Acceptance criteria

- `frontend/` directory exists with a working Vite + React + TS project
- Tailwind CSS configured with dark theme
- shadcn/ui initialized with base components installed
- React Router installed
- Path aliases (`@/`) working
- Vite dev proxy configured for BFF and Notifications service
- `npm run dev` and `npm run build` both succeed
