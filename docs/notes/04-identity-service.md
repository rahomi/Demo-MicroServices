---
ticket: "4"
title: "Minimal service boundary: fake identity endpoint"
type: "task"
date_completed: "2026-09-10"
status: "completed"
blocked_by: ["01-scaffold-solution"]
blocks: ["07-bff-service"]
tags: [ticket-completion, concept-tutorial]
---

# 🔑 Identity Service: Minimal Service Boundary

> [!abstract]
> **Core Idea**
>
> Not every microservice needs a database, messaging infrastructure, or MediatR. The Identity service demonstrates the **minimal service boundary** — a single endpoint returning a fake customer. It exists to show that service boundaries should match the service's responsibility, not a template.

---

## 🎯 Learning Objectives

- Understand when a service should be **intentionally minimal**
- Recognize that not every service needs a database or messaging
- Add Swagger UI even to minimal services for consistency

---

## 🧩 Main Concepts

### 1. The Minimal Service Boundary

#### Definition

A **service boundary** defines what a service is responsible for. The Identity service's responsibility is to provide customer identity information. In this demo, that's a single fake customer — no authentication, no database, no event handling.

#### Why It Exists

In a real system, Identity would handle authentication, JWT issuance, user management, etc. But for this demo, the BFF just needs a customer ID to route basket and order operations. A fake endpoint is the simplest solution that works.

#### Problem It Solves

Over-engineering. If every service had a database, MediatR, and RabbitMQ, the demo would be harder to understand. The Identity service shows that **service complexity should match service responsibility**.

> [!info]
> The Identity service references Contracts (from the scaffold) but doesn't use any RabbitMQ messaging or event DTOs. The reference is retained but unused — removing it would be cleaner but unnecessary for the demo.

---

### 2. Implementation

```csharp
// The entire service is one endpoint
app.MapGet("/api/customer", () =>
    new { Id = "cust-001", Name = "Test Customer", Email = "test@demo.local" })
   .WithSummary("Get current customer")
   .WithDescription("Returns the fake customer for the demo")
   .Produces<object>(StatusCodes.Status200OK);
```

> [!tip]
> The response is an anonymous object. No domain model or DTO is needed for a single static response — keeping the service intentionally minimal.

---

## 🛠️ Implementation Process

### Step 1 — Add Swashbuckle.AspNetCore
Only package needed (for Swagger UI consistency with other services)

### Step 2 — Create the endpoint
Single `GET /api/customer` returning a fixed anonymous object

### Step 3 — Add Swagger UI
Same `AddSwaggerGen` / `UseSwagger` / `UseSwaggerUI` as all other services

---

## 📊 Key Decisions

| Decision | Choice | Why |
|----------|--------|-----|
| Database | None | No persistence needed for a static response |
| Messaging | None | Identity doesn't publish or consume events |
| MediatR | None | Single endpoint — no complexity needed |
| Response type | Anonymous object | No domain model needed for static data |
| Swagger | Yes | Consistency with all other services |

---

## ✅ Testing & Verification

- [x] `dotnet build EcommerceDemo.slnx` — 0 warnings, 0 errors
- [x] `GET /api/customer` returns `{ "id": "cust-001", "name": "Test Customer", "email": "test@demo.local" }`
- [x] Swagger UI accessible at `/swagger`

---

## 📎 See Also

- [[01-scaffold-solution]] — Scaffold created the Identity project
- [[07-bff-service]] — BFF proxies the Identity endpoint

---

## 📝 Artifacts Created

- `EcommerceDemo/Identity/Program.cs` — Single endpoint + Swagger UI
- `EcommerceDemo/Identity/Identity.csproj` — Added Swashbuckle.AspNetCore
