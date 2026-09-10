using Microsoft.EntityFrameworkCore;
using Products.Domain;

namespace Products.Data;

/// <summary>
/// EF Core InMemory DbContext for the Products service.
/// </summary>
public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired();
            entity.Property(p => p.Price).HasPrecision(18, 2);
        });

        base.OnModelCreating(modelBuilder);
    }
}
