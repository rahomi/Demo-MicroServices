# T03 — RabbitMQ contracts and connection infrastructure design

**Labels:** `wayfinder:task`
**Parent:** [MAP — Chapter 19 Microservices Expansion](../MAP.md)
**Blocks:** T04, T05, T06
**Blocked by:** T01

## Question

What is the shape of the shared `Contracts` project — event DTOs, RabbitMQ connection setup, and the publish/consume abstractions — using direct `RabbitMQ.Client` without MassTransit?

### Detail

PLAN.md §4 requires a shared contracts/infrastructure area and direct `RabbitMQ.Client`. This ticket resolves:

1. **Contracts project** — Is it a class library (`C19.Contracts`)? What NuGet packages does it reference? (`RabbitMQ.Client` only, or also `System.Text.Json` for serialization?)
2. **Event DTOs** — Define the integration event contracts:
   - `OrderSubmitted` (OrderId, CustomerId, Items, Total, Timestamp)
   - `ProductChanged` (ProductId, Name, Price, ChangeType)
   - `BasketCheckedOut` (BasketId, CustomerId, Items)
   - Where do these live? (`C19.Contracts/Events/`)
3. **RabbitMQ connection setup** — Is there a shared `IRabbitMqConnection` / `RabbitMqConnection` class? Connection pooling? Reconnect logic?
4. **Publisher abstraction** — `IEventPublisher` interface with a `PublishAsync<T>(string routingKey, T @event)` method? Or raw channel-per-publish?
5. **Consumer abstraction** — How do services register consumers? A `BackgroundService` that subscribes to a queue? An `IHostedService` pattern?
6. **Serialization** — `System.Text.Json` with `JsonSerializer`? Event type name as routing key or as a `type` header?
7. **Exchange/queue topology** — Direct exchange? Topic exchange? One exchange per event type? Queue naming convention?

### Resolution

*(To be filled when resolved — record the Contracts project structure, event DTO definitions, connection/publisher/consumer abstractions, and topology decisions.)*
