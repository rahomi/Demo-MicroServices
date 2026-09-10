using Baskets.Domain;
using Microsoft.EntityFrameworkCore;

namespace Baskets.Data;

/// <summary>
/// EF Core InMemory DbContext for the Baskets service.
/// </summary>
public class BasketDbContext : DbContext
{
    public BasketDbContext(DbContextOptions<BasketDbContext> options) : base(options)
    {
    }

    public DbSet<Basket> Baskets => Set<Basket>();
    public DbSet<BasketItem> BasketItems => Set<BasketItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Basket>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.CustomerId).IsRequired();
            entity.HasMany(b => b.Items)
                .WithOne()
                .HasForeignKey(i => i.BasketId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BasketItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.ProductName).IsRequired();
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
        });

        base.OnModelCreating(modelBuilder);
    }
}
