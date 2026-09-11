using System.Text.Json;

namespace BFF.Saga;

/// <summary>
/// Saga state machine values for the checkout saga.
/// </summary>
public enum SagaStatus
{
    Started,
    BasketReserved,
    OrderCreated,
    Completed,
    Compensating,
    Failed
}

/// <summary>
/// A snapshot of basket items captured at checkout time, used for compensation.
/// </summary>
public record BasketItemSnapshot(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);

/// <summary>
/// Persisted saga state for the checkout saga. Stored in the BFF's EF Core InMemory database.
/// </summary>
public class SagaState
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public SagaStatus CurrentState { get; set; } = SagaStatus.Started;

    /// <summary>
    /// JSON-serialized snapshot of basket items captured at checkout (for compensation).
    /// </summary>
    public string BasketSnapshotJson { get; set; } = "[]";

    /// <summary>
    /// The order ID created during the saga (null until Step 2 succeeds).
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Error message if the saga failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Deserializes the basket snapshot from JSON.
    /// </summary>
    public List<BasketItemSnapshot> GetBasketSnapshot() =>
        string.IsNullOrEmpty(BasketSnapshotJson)
            ? []
            : JsonSerializer.Deserialize<List<BasketItemSnapshot>>(BasketSnapshotJson) ?? [];

    /// <summary>
    /// Serializes basket items into the snapshot JSON.
    /// </summary>
    public void SetBasketSnapshot(List<BasketItemSnapshot> items) =>
        BasketSnapshotJson = JsonSerializer.Serialize(items);
}
