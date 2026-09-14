using BFF.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BFF.Saga;

/// <summary>
/// Result of a saga execution.
/// </summary>
public record SagaResult(
    Guid SagaId,
    SagaStatus FinalState,
    OrderDto? Order,
    string? ErrorMessage);

/// <summary>
/// Orchestrates the checkout saga:
///   Step 1 — Reserve Basket (checkout/clear basket, capture snapshot)
///   Step 2 — Create Order (submit order to Orders service)
///   Step 3 — Complete (mark saga Completed)
/// 
/// Compensating actions on failure:
///   If Step 2 fails → restore basket items from snapshot
///   If Step 3 fails → cancel order + restore basket items
/// </summary>
public class CheckoutSagaOrchestrator
{
    private readonly SagaDbContext _sagaDb;
    private readonly IBasketsClient _basketsClient;
    private readonly IOrdersClient _ordersClient;
    private readonly ILogger<CheckoutSagaOrchestrator> _logger;

    public CheckoutSagaOrchestrator(
        SagaDbContext sagaDb,
        IBasketsClient basketsClient,
        IOrdersClient ordersClient,
        ILogger<CheckoutSagaOrchestrator> logger)
    {
        _sagaDb = sagaDb;
        _basketsClient = basketsClient;
        _ordersClient = ordersClient;
        _logger = logger;
    }

    /// <summary>
    /// Executes the checkout saga for the given customer.
    /// </summary>
    public async Task<SagaResult> ExecuteAsync(string customerId, CancellationToken ct = default)
    {
        // --- Create saga state ---
        var saga = new SagaState
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CurrentState = SagaStatus.Started,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _sagaDb.Sagas.Add(saga);

        _logger.LogInformation("Saga {SagaId} started for customer {CustomerId}.", saga.Id, customerId);

        // --- Step 1: Reserve Basket (checkout) ---
        CheckoutResult checkout;
        try
        {
            checkout = await _basketsClient.CheckoutAsync(customerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Saga {SagaId}: Step 1 (Basket checkout) failed for customer {CustomerId}.", saga.Id, customerId);
            // No compensation needed — basket was not cleared
            await FailSagaAsync(saga, "Basket checkout failed: " + ex.Message, ct);
            return new SagaResult(saga.Id, SagaStatus.Failed, null, "Basket checkout failed");
        }

        if (checkout.Items.Count == 0)
        {
            _logger.LogWarning("Saga {SagaId}: Basket is empty — nothing to checkout.", saga.Id);
            await FailSagaAsync(saga, "Basket is empty", ct);
            return new SagaResult(saga.Id, SagaStatus.Failed, null, "Basket is empty — nothing to checkout");
        }

        // Capture snapshot for compensation
        var snapshot = checkout.Items
            .Select(i => new BasketItemSnapshot(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity))
            .ToList();
        saga.SetBasketSnapshot(snapshot);
        saga.CurrentState = SagaStatus.BasketReserved;

        _logger.LogInformation("Saga {SagaId}: Step 1 complete — basket reserved. {ItemCount} items captured in snapshot.",
            saga.Id, snapshot.Count);

        // --- Step 2: Create Order ---
        OrderDto? order = null;
        try
        {
            var orderRequest = new SubmitOrderRequest(
                checkout.CustomerId,
                checkout.Items.Select(i => new SubmitOrderItem(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList()
            );

            order = await _ordersClient.SubmitOrderAsync(orderRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Saga {SagaId}: Step 2 (Order creation) failed. Starting compensation.", saga.Id);

            // Compensate: restore basket items
            await CompensateAsync(saga, restoreBasket: true, cancelOrder: false, ct);
            return new SagaResult(saga.Id, SagaStatus.Failed, null, "Order creation failed; basket restored");
        }

        saga.OrderId = order.Id;

        _logger.LogInformation("Saga {SagaId}: Step 2 complete — order {OrderId} created.", saga.Id, order.Id);

        // --- Step 3: Complete — single save with final state ---
        saga.CurrentState = SagaStatus.Completed;
        saga.UpdatedAt = DateTime.UtcNow;
        await _sagaDb.SaveChangesAsync(ct);

        _logger.LogInformation("Saga {SagaId}: Step 3 complete — saga finished successfully. Order {OrderId}.", saga.Id, order.Id);

        return new SagaResult(saga.Id, SagaStatus.Completed, order, null);
    }

    /// <summary>
    /// Runs compensating actions in reverse order.
    /// </summary>
    private async Task CompensateAsync(SagaState saga, bool restoreBasket, bool cancelOrder, CancellationToken ct)
    {
        saga.CurrentState = SagaStatus.Compensating;

        _logger.LogWarning("Saga {SagaId}: Compensating — restoreBasket={RestoreBasket}, cancelOrder={CancelOrder}.",
            saga.Id, restoreBasket, cancelOrder);

        // Compensate Step 2: cancel order
        if (cancelOrder && saga.OrderId.HasValue)
        {
            try
            {
                await _ordersClient.CancelOrderAsync(saga.OrderId.Value);
                _logger.LogInformation("Saga {SagaId}: Compensation — order {OrderId} cancelled.", saga.Id, saga.OrderId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saga {SagaId}: Compensation failed — could not cancel order {OrderId}.", saga.Id, saga.OrderId);
            }
        }

        // Compensate Step 1: restore basket items
        if (restoreBasket)
        {
            try
            {
                var snapshot = saga.GetBasketSnapshot();
                var restoreItems = snapshot
                    .Select(s => new RestoreBasketItemRequest(s.ProductId, s.ProductName, s.UnitPrice, s.Quantity))
                    .ToList();

                await _basketsClient.RestoreBasketAsync(saga.CustomerId, restoreItems);
                _logger.LogInformation("Saga {SagaId}: Compensation — basket restored with {ItemCount} items.", saga.Id, restoreItems.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saga {SagaId}: Compensation failed — could not restore basket.", saga.Id);
            }
        }

        saga.CurrentState = SagaStatus.Failed;
        saga.UpdatedAt = DateTime.UtcNow;
        await _sagaDb.SaveChangesAsync(ct);

        _logger.LogWarning("Saga {SagaId}: Compensation complete — saga marked as Failed.", saga.Id);
    }

    /// <summary>
    /// Retrieves a saga by ID (for the GET /api/sagas/{id} endpoint).
    /// </summary>
    public async Task<SagaState?> GetSagaAsync(Guid id, CancellationToken ct = default)
    {
        return await _sagaDb.Sagas.FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    private async Task FailSagaAsync(SagaState saga, string errorMessage, CancellationToken ct)
    {
        saga.CurrentState = SagaStatus.Failed;
        saga.ErrorMessage = errorMessage;
        saga.UpdatedAt = DateTime.UtcNow;
        await _sagaDb.SaveChangesAsync(ct);
    }
}
