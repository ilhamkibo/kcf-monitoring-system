using Microsoft.EntityFrameworkCore;
using KcfMonitoringSystem.Domain.Entities;
using KcfMonitoringSystem.Infrastructure.Persistence;

namespace KcfMonitoringSystem.Infrastructure.Data;

public static class ProductionSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Productions.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        // ambil semua operator yang punya machine
        var operators = await db.Users
            .Where(u => u.Role == "Operator" && u.MachineId != null)
            .ToListAsync();

        var productions = new List<Production>();

        foreach (var user in operators)
        {
            productions.Add(new Production
            {
                UserId = user.Id,
                MachineId = user.MachineId!.Value,
                Quantity = Random.Shared.Next(100, 500),
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        db.Productions.AddRange(productions);

        await db.SaveChangesAsync();
    }
}