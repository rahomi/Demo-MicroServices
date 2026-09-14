# F12 — Docker integration: Dockerfile, nginx.conf, docker-compose

**Labels:** `wayfinder:map`, `wayfinder:task`
**Parent:** [MAP — Frontend Implementation](../FRONTEND-MAP.md)
**Blocks:** —
**Blocked by:** F04

## Question

Containerize the frontend and integrate it into the existing `docker-compose.yml` so the full stack runs with a single `docker compose up`.

### Tasks

1. **Dockerfile** (multi-stage) at `frontend/Dockerfile`:
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

2. **nginx.conf** at `frontend/nginx.conf`:
   - Serve static files from `/usr/share/nginx/html`
   - Docker DNS resolver to prevent crashes on container restart:
     ```nginx
     resolver 127.0.0.11 valid=10s;
     ```
   - Variable-based upstreams for dynamic DNS resolution:
     ```nginx
     set $bff_host bff;
     set $notifications_host notifications;
     ```
   - `location /api/notifications/` → `proxy_pass http://$notifications_host:5004` (more specific, takes precedence)
   - `location /api/` → `proxy_pass http://$bff_host:5000`
   - SPA fallback: `try_files $uri $uri/ /index.html`

3. **docker-compose.yml** — Add frontend service:
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

4. **.dockerignore** at `frontend/.dockerignore`:
   - `node_modules`
   - `dist`
   - `.git`

5. **Verify** — `docker compose up --build` starts all services including frontend on port 3000. Frontend loads, API calls proxy correctly to BFF and Notifications service.

6. **Update README** — Add frontend to the service URLs table and quick start section.

### Acceptance criteria

- `frontend/Dockerfile` builds successfully (multi-stage)
- `nginx.conf` serves static files and proxies API calls with DNS resolver
- `docker-compose.yml` includes frontend service on port 3000
- `docker compose up --build` starts everything, frontend accessible at `http://localhost:3000`
- API calls from the frontend reach BFF (port 5000) and Notifications (port 5004) correctly
- SPA routing works (direct navigation to `/shop/products` doesn't 404)
- README updated with frontend service info
