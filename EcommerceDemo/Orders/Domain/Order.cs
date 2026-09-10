namespace Orders.Domain;

/// <summary>
/// Order entity stored in the Orders service's EF Core InMemory database.
/// </summary>
public class Order
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = [];
    public decimal Total { get; set; }
    public string Status { get; set; } = "Submitted";
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// A line item within an order.
/// </summary>
public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
