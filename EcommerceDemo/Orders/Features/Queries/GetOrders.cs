using Contracts.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Orders.Data;
using Orders.Domain;

namespace Orders.Features.Queries;

/// <summary>
/// Query to get a single order by ID.
/// </summary>
public record GetOrderByIdQuery(Guid Id) : IRequest<Order?>;

/// <summary>
/// Query to get all orders for a customer.
/// </summary>
public record GetOrdersByCustomerQuery(string CustomerId) : IRequest<List<Order>>;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, Order?>
{
    private readonly OrderDbContext _db;

    public GetOrderByIdHandler(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<Order?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        return await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
    }
}

public class GetOrdersByCustomerHandler : IRequestHandler<GetOrdersByCustomerQuery, List<Order>>
{
    private readonly OrderDbContext _db;

    public GetOrdersByCustomerHandler(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<List<Order>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        return await _db.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == request.CustomerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
