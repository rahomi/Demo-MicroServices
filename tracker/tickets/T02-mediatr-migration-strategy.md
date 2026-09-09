# T02 — MediatR migration strategy for Products and Baskets

**Labels:** `wayfinder:task`
**Parent:** [MAP — Chapter 19 Microservices Expansion](../MAP.md)
**Blocks:** T04
**Blocked by:** T01

## Question

How are the existing Products and Baskets manual handlers migrated to MediatR (`IRequest`, `IRequestHandler`), and what is the canonical feature-folder layout for commands, queries, validators, and mappings?

### Detail

PLAN.md §2 requires migrating Products and Baskets to MediatR. This ticket resolves:

1. **MediatR registration** — How is MediatR registered in each service's `Program.cs`? (`services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(...))`)
2. **Feature-folder convention** — Confirm the layout per service:
   ```
   Products/
     Features/
       Products/
         GetProducts.cs        (query + handler)
         GetProductById.cs     (query + handler)
         CreateProduct.cs      (command + handler)
         UpdateProduct.cs      (command + handler)
         DeleteProduct.cs      (command + handler)
   ```
3. **Validator placement** — FluentValidation validators alongside commands/queries, or in a `Validators/` subfolder?
4. **Mapping** — Is there a manual mapping or AutoMapper? Where do mappings live?
5. **Endpoint mapping** — How do Minimal API endpoints dispatch via `IMediator`? (`await mediator.Send(new GetProducts.Query())`)
6. **`Features.cs` reuse** — PLAN.md references `Features.cs` for feature registration and endpoint mapping. Confirm its role and shape.

### Resolution

*(To be filled when resolved — record the MediatR registration snippet, feature-folder layout, and endpoint dispatch pattern.)*
