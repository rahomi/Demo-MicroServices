# T04 — BFF routing map and Refit client contracts

**Labels:** `wayfinder:task`
**Parent:** [MAP — Chapter 19 Microservices Expansion](../MAP.md)
**Blocks:** T08
**Blocked by:** T02, T03

## Question

What are the BFF's downstream routes, Refit client interfaces, and endpoint-to-service mapping for the expanded service set (Products, Baskets, Orders, Identity)?

### Detail

PLAN.md §6 requires adding/updating BFF routes for checkout and order retrieval, and updating Refit clients and contracts. This ticket resolves:

1. **Refit client interfaces** — What does `IWebClient.cs` (or per-service interfaces) look like?
   - `IProductsClient` — GetProducts, GetProductById, CreateProduct, UpdateProduct, DeleteProduct
   - `IBasketsClient` — GetBasket, AddItem, RemoveItem, Checkout
   - `IOrdersClient` — GetOrderById, GetOrdersByCustomer
   - `IIdentityClient` — GetCurrentCustomer (returns fake customer)
2. **BFF endpoint map** — What routes does the BFF expose?
   - `GET /api/products` → Products
   - `GET /api/products/{id}` → Products
   - `POST /api/products` → Products
   - `PUT /api/products/{id}` → Products
   - `DELETE /api/products/{id}` → Products
   - `GET /api/baskets/{customerId}` → Baskets
   - `POST /api/baskets/{customerId}/items` → Baskets
   - `DELETE /api/baskets/{customerId}/items/{productId}` → Baskets
   - `POST /api/baskets/{customerId}/checkout` → Baskets (triggers OrderSubmitted)
   - `GET /api/orders/{id}` → Orders
   - `GET /api/orders?customerId={id}` → Orders
   - `GET /api/identity/customer` → Identity
3. **Service URL configuration** — How are downstream service URLs configured? (`HttpClient` base address from environment/config, Refit `AddRefitClient<T>()`)
4. **Checkout flow** — Does the BFF orchestrate checkout (call Baskets → call Orders → return), or does Baskets publish an event and BFF polls?
5. **Error propagation** — How are downstream errors surfaced? (Pass through status codes, wrap in ProblemDetails?)

### Resolution

*(To be filled when resolved — record the Refit client interfaces, BFF endpoint map, service URL config, and checkout orchestration decision.)*
