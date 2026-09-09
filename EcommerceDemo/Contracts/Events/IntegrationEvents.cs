namespace Contracts.Events;

/// <summary>
/// Published when an order is submitted/created.
/// </summary>
public record OrderSubmitted(
    Guid OrderId,
    string CustomerId,
    decimal Total,
    List<OrderItemDto> Items,
    DateTime CreatedAt);

/// <summary>
/// Published when a product is created, updated, or deleted.
/// </summary>
public record ProductChanged(
    Guid ProductId,
    string Name,
    decimal Price,
    string ChangeType);

/// <summary>
/// Published when a basket is checked out.
/// </summary>
public record BasketCheckedOut(
    string CustomerId,
    List<BasketItemDto> Items,
    DateTime CheckedOutAt);

public record OrderItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
public record BasketItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
