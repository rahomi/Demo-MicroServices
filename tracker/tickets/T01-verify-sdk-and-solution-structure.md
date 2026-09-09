# T01 — Verify .NET SDK and establish solution/project structure

**Labels:** `wayfinder:map`, `wayfinder:research`
**Parent:** [MAP — Chapter 19 Microservices Expansion](../MAP.md)
**Blocks:** T02, T03, T05, T07
**Blocked by:** — (frontier)

## Question

What .NET SDK is installed on the development machine, and what is the canonical solution and project structure for the expanded Chapter 19?

### Detail

The PLAN.md says to verify the installed latest stable .NET SDK and use it when compatible, otherwise retain .NET 8. This ticket resolves:

1. **SDK version** — Run `dotnet --version` and `dotnet --list-sdks`. Record the installed SDK(s).
2. **Target framework** — Decide: .NET 8 or newer? This fixes every `.csproj`.
3. **Solution structure** — Confirm the project layout:
   - `C19.sln` at repo root
   - `C19/BFF/` — Backend for Frontend
   - `C19/Products/` — Catalog service
   - `C19/Baskets/` — Basket service
   - `C19/Orders/` — Orders service (new)
   - `C19/Notifications/` — Notifications service (new)
   - `C19/Identity/` — Fake identity service (new)
   - `C19/Contracts/` — Shared event DTOs and RabbitMQ infrastructure (new)
4. **Project references** — Which projects reference `Contracts`? (All services that publish or consume events.)
5. **Shared build conventions** — `Directory.Build.props` for common `<TargetFramework>`, `<Nullable>`, `<ImplicitUsings>`.
6. sln file 
### Resolution

**SDK version:** .NET 9.0.313 and .NET 10.0.301 installed. Selected .NET 10 (latest stable).

**Target framework:** `net10.0`

**Solution file:** `EcommerceDemo.slnx` (new XML solution format, created by `dotnet new sln` on .NET 10)

**Confirmed project tree:**
```
EcommerceDemo/
├── Contracts/          — Shared event DTOs and RabbitMQ infrastructure (class library)
├── BFF/                — Backend for Frontend
├── Products/           — Catalog service
├── Baskets/            — Basket service
├── Orders/             — Orders service (new)
├── Notifications/      — Notifications service (new)
└── Identity/           — Fake identity service (new)
```

**Project references:** All 6 service projects reference `Contracts`. No service-to-service references.

**Shared build conventions:** `Directory.Build.props` at repo root with `<TargetFramework>net10.0</TargetFramework>`, `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`, `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.

**Status:** ✅ Resolved. See [[01-scaffold-solution]] for the completion note.
