---
title: "API documentation: .http files + comprehensive README"
---

# 📖 API Documentation: .http Files + Comprehensive README

> [!abstract]
> **Core Idea**
>
> A new user should be able to clone the repo, run `docker compose up --build`, and verify the entire system works in 10 steps. This note covers the `api.http` file (interactive API reference for VS Code REST Client) and the `README.md` (one-command startup, routing map, tracing, saga, and verification checklist).

---

## 🎯 Learning Objectives

- Structure an `api.http` file with **variables and sections** for all API operations
- Write a README that covers **startup, routing, tracing, saga, and verification**
- Create a **10-step demo sequence** that mirrors the  verification checklist
- Document the **saga failure simulation** (stop orders container, checkout, verify basket restored)

---

## 🧩 Main Concepts

### 1. The .http File as Interactive API Reference

#### Definition

A `.http` file is a plain-text format recognized by VS Code's REST Client extension. It lets you define HTTP requests with variables, sections, and comments — then execute them with a single click.

#### Structure

```http
### Variables
@bff = http://localhost:5000
@products = http://localhost:5001

### 1. Product Operations
### Get all products
GET {{bff}}/api/products

### Create a product
POST {{bff}}/api/products
Content-Type: application/json

{
  "name": "Gaming Mouse",
  "price": 59.99,
  "category": "Peripherals"
}

### 2. Basket Operations
### Add item to basket
POST {{bff}}/api/baskets/cust-001/items
Content-Type: application/json

{
  "productId": "{{$guid}}",
  "productName": "Wireless Mouse",
  "unitPrice": 29.99,
  "quantity": 2
}

### 3. Checkout (Saga)
### Start checkout — triggers saga orchestration
POST {{bff}}/api/baskets/cust-001/checkout

### Inspect saga state
GET {{bff}}/api/sagas/{{sagaId}}
```

> [!tip]
> Using `@bff` variable means you can switch between calling the BFF (port 5000) or a downstream service directly by changing one variable.

---

### 2. The 10-Step Demo Sequence

#### Definition

A scripted sequence of API calls that demonstrates the full system — from product creation to saga failure simulation.

| Step | Action | What It Demonstrates |
|------|--------|---------------------|
| 1 | `GET /api/products` | Products service + seed data |
| 2 | `POST /api/products` | Create + `ProductChanged` event |
| 3 | `GET /api/notifications` | Event-driven architecture (event appeared) |
| 4 | `POST /api/baskets/cust-001/items` | Basket operations |
| 5 | `POST /api/baskets/cust-001/checkout` | Saga orchestration |
| 6 | `GET /api/sagas/{id}` | Saga state inspection |
| 7 | `docker compose stop orders` | Failure simulation |
| 8 | `POST /api/baskets/cust-001/checkout` | Saga compensation |
| 9 | `GET /api/sagas/{id}` | Saga state = Failed, basket restored |
| 10 | Open Jaeger UI | Distributed trace tree |

---

### 3. README Structure

```markdown
# EcommerceDemo — Microservices with Saga + Distributed Tracing

## Quick Start
docker compose up --build

## Service URLs
| Service | URL | Port |
|---------|-----|------|
| BFF (Swagger) | http://localhost:5000/swagger | 5000 |
| RabbitMQ UI | http://localhost:15672 | 15672 |
| Jaeger UI | http://localhost:16686 | 16686 |

## Architecture
[ASCII diagram of 8-container topology]

## Routing Map
[BFF route → downstream service]

## Event Flow
[checkout → BasketCheckedOut → OrderSubmitted → Notifications]

## Saga Pattern
[State machine + compensation flow]

## Distributed Tracing
[How traceparent crosses HTTP + RabbitMQ]

## 10-Step Verification
[Mirrors  checklist]
```

---

## 🛠️ Implementation Process

### Step 1 — Create `api.http` at repo root
8 sections: products, baskets, checkout, orders, event-driven, saga failure, identity, full demo

### Step 2 — Create `README.md` at repo root
Cover: startup, service URLs, architecture, routing, RabbitMQ, Jaeger, sample sequence, event flow, saga, tracing, local dev, verification checklist, project structure, technologies

### Step 3 — Verify build
`dotnet build EcommerceDemo.slnx` — 0 errors, 0 warnings

### Step 4 — Developer confirmation (GATE)
Developer confirmed "yes" at the Step 5 gate

---

## 📊 Key Decisions

| Decision | Choice | Why |
|----------|--------|-----|
| Single `api.http` at root | Not scattered per-service | Complete, navigable API reference in one file |
| README covers Docker + local dev | Both paths |  requires both |
| curl commands in README | Not just .http | Copy-paste in any terminal, no VS Code needed |
| 10-step sequence | Mirrors  | Easy to demo the full flow |

---

## ✅ Testing & Verification

- [x] `dotnet build EcommerceDemo.slnx` — 0 errors, 0 warnings
- [x] Developer confirmed "yes" at the Step 5 gate

---

## 📎 See Also

- [[03-cqrs-with-mediatr]] — Products endpoints documented
- [[04-basket-operations-and-event-consumer]] — Baskets endpoints documented
- [[05-minimal-service-boundary]] — Identity endpoint documented
- [[06-order-submission-and-events]] — Orders endpoints documented
- [[07-event-driven-consumer-pattern]] — Notifications endpoint documented
- [[08-backend-for-frontend-pattern]] — BFF routing map documented
- [[09-saga-pattern-with-compensation]] — Saga pattern documented in README
- [[10-distributed-tracing-with-opentelemetry]] — Tracing documented in README
- [[11-docker-compose-and-containerization]] — Docker Compose startup documented in README

---

