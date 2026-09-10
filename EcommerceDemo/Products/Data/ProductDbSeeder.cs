using Microsoft.EntityFrameworkCore;
using Products.Domain;

namespace Products.Data;

/// <summary>
/// Seeds the Products InMemory database with sample products on startup.
/// </summary>
public static class ProductDbSeeder
{
    public static async Task SeedAsync(ProductDbContext context)
    {
        if (await context.Products.AnyAsync())
            return;

        var products = new[]
        {
            new Product { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Wireless Mouse", Price = 29.99m, Category = "Electronics" },
            new Product { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Mechanical Keyboard", Price = 89.99m, Category = "Electronics" },
            new Product { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "USB-C Hub", Price = 49.99m, Category = "Electronics" },
            new Product { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "27-inch Monitor", Price = 299.99m, Category = "Electronics" },
            new Product { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Laptop Stand", Price = 24.99m, Category = "Accessories" }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
}
