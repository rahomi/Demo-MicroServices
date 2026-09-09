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

**Resolved 2026-09-09.** The Contracts project was already implemented during T01 (scaffold). This ticket confirms and documents the design decisions.

#### 1. Contracts project

- **Project:** `EcommerceDemo/Contracts/` — a class library targeting `net10.0`.
- **NuGet packages:** `RabbitMQ.Client 7.2.2`, `Microsoft.Extensions.Logging.Abstractions`, `Microsoft.Extensions.Configuration.Abstractions`, `Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.Extensions.Hosting.Abstractions`.
- **Namespace:** `Contracts` (sub-namespaces: `Contracts.Events`, `Contracts.Messaging`).

#### 2. Event DTOs (`Contracts/Events/IntegrationEvents.cs`)

All events are C# `record` types for immutability and value equality:

| Event | Fields |
|-------|--------|
| `OrderSubmitted` | `Guid OrderId`, `string CustomerId`, `decimal Total`, `List<OrderItemDto> Items`, `DateTime CreatedAt` |
| `ProductChanged` | `Guid ProductId`, `string Name`, `decimal Price`, `string ChangeType` |
| `BasketCheckedOut` | `string CustomerId`, `List<BasketItemDto> Items`, `DateTime CheckedOutAt` |
| `OrderItemDto` | `Guid ProductId`, `string ProductName`, `decimal UnitPrice`, `int Quantity` |
| `BasketItemDto` | `Guid ProductId`, `string ProductName`, `decimal UnitPrice`, `int Quantity` |

#### 3. RabbitMQ connection (`IRabbitMqConnection` / `RabbitMqConnection`)

- Singleton registered in DI via `AddRabbitMqMessaging()`.
- Wraps `IConnectionFactory` / `IConnection` from `RabbitMQ.Client`.
- `TryConnectAsync()` — semaphore-guarded reconnect; idempotent if already connected.
- `CreateChannelAsync()` — ensures connection, then creates a new channel.
- Subscribes to `ConnectionShutdownAsync` to log and reconnect on next use (lazy reconnect, no polling).

#### 4. Publisher abstraction (`IEventPublisher` / `EventPublisher`)

```csharp
Task PublishAsync<T>(string exchange, string routingKey, T @event, CancellationToken ct = default) where T : class;
```

- Creates a channel per publish (short-lived), serializes with `System.Text.Json`, sets `ContentType = "application/json"`, `DeliveryMode = Persistent`.
- **Distributed tracing:** starts an `ActivitySource` ("RabbitMQ") producer span, injects W3C `traceparent` into `BasicProperties.Headers` so consumers can link spans.

#### 5. Consumer abstraction (`EventConsumer<T>`)

- Abstract `BackgroundService` base class — each service subclasses it and implements `HandleAsync(T @event, CancellationToken ct)`.
- Declares a durable queue, binds to `amq.topic` exchange with the routing key, sets `BasicQos` prefetch = 1.
- **Distributed tracing:** extracts `traceparent` from headers, creates a linked consumer activity.
- Auto-ack on success, nack (no requeue) on failure.

#### 6. Serialization

- `System.Text.Json` (`JsonSerializer.Serialize` / `Deserialize`).
- Routing key = event type name (e.g., `"product.changed"`, `"order.submitted"`, `"basket.checkedout"`).

#### 7. Exchange / queue topology

- **Exchange:** `amq.topic` (built-in topic exchange). Topic exchange chosen over direct so routing keys can use dot-separated patterns.
- **Queue naming:** per-consumer, e.g., `notifications.product.changed`, `notifications.order.submitted`, `baskets.product.changed`.
- **Routing keys:** dot-separated event names: `product.changed`, `order.submitted`, `basket.checkedout`.

#### DI registration

```csharp
builder.Services.AddRabbitMqMessaging(builder.Configuration);
```

Reads `RabbitMq:Host`, `RabbitMq:Port`, `RabbitMq:UserName`, `RabbitMq:Password` from configuration (defaults: `localhost:5672`, `guest/guest`).
