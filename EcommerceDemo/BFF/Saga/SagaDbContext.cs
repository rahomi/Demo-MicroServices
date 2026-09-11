using Microsoft.EntityFrameworkCore;

namespace BFF.Saga;

/// <summary>
/// EF Core InMemory DbContext for saga state persistence in the BFF.
/// </summary>
public class SagaDbContext : DbContext
{
    public SagaDbContext(DbContextOptions<SagaDbContext> options) : base(options)
    {
    }

    public DbSet<SagaState> Sagas => Set<SagaState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SagaState>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.CustomerId).IsRequired();
            entity.Property(s => s.CurrentState).HasConversion<string>();
            entity.Property(s => s.BasketSnapshotJson).IsRequired();
            entity.Property(s => s.ErrorMessage).HasMaxLength(1000);
        });

        base.OnModelCreating(modelBuilder);
    }
}
