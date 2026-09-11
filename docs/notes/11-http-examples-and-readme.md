---
ticket: "11"
title: "API documentation: .http files + comprehensive README"
type: "task"
date_completed: "2026-09-11"
status: "completed"
blocked_by: ["10-docker-compose", "08-saga-orchestration", "09-distributed-tracing"]
blocks: []
tags: [ticket-completion, concept-tutorial]
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
- Create a **10-step demo sequence** that mirrors the PLAN.md verification checklist
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
[Mirrors PLAN.md checklist]
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
| README covers Docker + local dev | Both paths | PLAN.md requires both |
| curl commands in README | Not just .http | Copy-paste in any terminal, no VS Code needed |
| 10-step sequence | Mirrors PLAN.md | Easy to demo the full flow |

---

## ✅ Testing & Verification

- [x] `dotnet build EcommerceDemo.slnx` — 0 errors, 0 warnings
- [x] Developer confirmed "yes" at the Step 5 gate

---

## 📎 See Also

- [[02-products-service]] — Products endpoints documented
- [[03-baskets-service]] — Baskets endpoints documented
- [[04-identity-service]] — Identity endpoint documented
- [[05-orders-service]] — Orders endpoints documented
- [[06-notifications-service]] — Notifications endpoint documented
- [[07-bff-service]] — BFF routing map documented
- [[08-saga-orchestration]] — Saga pattern documented in README
- [[09-distributed-tracing]] — Tracing documented in README
- [[10-docker-compose]] — Docker Compose startup documented in README

---

## 📝 Artifacts Created

- `api.http` — HTTP examples for all API operations (8 sections, full demo sequence)
- `README.md` — Comprehensive project documentation with startup, routing, tracing, saga, verification
