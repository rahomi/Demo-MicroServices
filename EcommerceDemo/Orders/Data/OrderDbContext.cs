using Microsoft.EntityFrameworkCore;
using Orders.Domain;

namespace Orders.Data;

/// <summary>
/// EF Core InMemory DbContext for the Orders service.
/// </summary>
public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.CustomerId).IsRequired();
            entity.Property(o => o.Total).HasPrecision(18, 2);
            entity.Property(o => o.Status).IsRequired();
            entity.HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(i => i.OrderId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.ProductName).IsRequired();
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
        });

        base.OnModelCreating(modelBuilder);
    }
}
