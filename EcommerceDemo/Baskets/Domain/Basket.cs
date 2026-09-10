namespace Baskets.Domain;

/// <summary>
/// A shopping basket belonging to a customer, stored in the Baskets service's EF Core InMemory database.
/// </summary>
public class Basket
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public List<BasketItem> Items { get; set; } = [];
}

/// <summary>
/// A single line item in a basket.
/// </summary>
public class BasketItem
{
    public Guid Id { get; set; }
    public Guid BasketId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
