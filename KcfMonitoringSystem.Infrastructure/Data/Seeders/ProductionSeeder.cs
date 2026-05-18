using Microsoft.EntityFrameworkCore;
using KcfMonitoringSystem.Domain.Entities;
using KcfMonitoringSystem.Infrastructure.Persistence;

namespace KcfMonitoringSystem.Infrastructure.Data;

public static class ProductionSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Productions.AnyAsync())
        {
            var existingProductions = await db.Productions.Where(p => p.ProductId == null).ToListAsync();
            if (existingProductions.Any())
            {
                var allProducts = await db.Products.ToListAsync();
                if (allProducts.Any())
                {
                    foreach (var prod in existingProductions)
                    {
                        prod.ProductId = allProducts[Random.Shared.Next(allProducts.Count)].Id;
                    }
                    await db.SaveChangesAsync();
                }
            }
            return;
        }

        var now = DateTime.UtcNow;

        // ambil semua operator yang punya machine
        var operators = await db.Users
            .Where(u => u.Role == "Operator" && u.MachineId != null)
            .ToListAsync();

        // ambil semua product untuk random assignment
        var products = await db.Products.ToListAsync();

        var productions = new List<Production>();

        foreach (var user in operators)
        {
            // pilih random product
            var productId = products.Count > 0
                ? products[Random.Shared.Next(products.Count)].Id
                : (int?)null;

            productions.Add(new Production
            {
                UserId = user.Id,
                MachineId = user.MachineId!.Value,
                ProductId = productId,
                Quantity = Random.Shared.Next(100, 500),
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        db.Productions.AddRange(productions);

        await db.SaveChangesAsync();
    }
}