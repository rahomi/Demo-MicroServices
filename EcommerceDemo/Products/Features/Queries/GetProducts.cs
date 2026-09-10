using MediatR;
using Microsoft.EntityFrameworkCore;
using Products.Domain;

namespace Products.Features.Queries;

/// <summary>
/// Query to get all products.
/// </summary>
public record GetProductsQuery : IRequest<List<Product>>;

/// <summary>
/// Query to get a single product by ID.
/// </summary>
public record GetProductByIdQuery(Guid Id) : IRequest<Product?>;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, List<Product>>
{
    private readonly Data.ProductDbContext _db;

    public GetProductsHandler(Data.ProductDbContext db)
    {
        _db = db;
    }

    public async Task<List<Product>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        return await _db.Products.ToListAsync(cancellationToken);
    }
}

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, Product?>
{
    private readonly Data.ProductDbContext _db;

    public GetProductByIdHandler(Data.ProductDbContext db)
    {
        _db = db;
    }

    public async Task<Product?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        return await _db.Products.FindAsync([request.Id], cancellationToken);
    }
}
