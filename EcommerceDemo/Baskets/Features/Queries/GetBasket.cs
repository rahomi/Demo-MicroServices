using Baskets.Data;
using Baskets.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baskets.Features.Queries;

/// <summary>
/// Query to get a basket by customer ID.
/// </summary>
public record GetBasketQuery(string CustomerId) : IRequest<Basket?>;

public class GetBasketHandler : IRequestHandler<GetBasketQuery, Basket?>
{
    private readonly BasketDbContext _db;

    public GetBasketHandler(BasketDbContext db)
    {
        _db = db;
    }

    public async Task<Basket?> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        return await _db.Baskets
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.CustomerId == request.CustomerId, cancellationToken);
    }
}
