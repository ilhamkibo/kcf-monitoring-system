using Microsoft.EntityFrameworkCore;
using KcfMonitoringSystem.Domain.Entities;
using KcfMonitoringSystem.Infrastructure.Persistence;

namespace KcfMonitoringSystem.Infrastructure.Data;

public static class GroupSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Groups.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        db.Groups.AddRange(
            new Group { Name = "A", CreatedAt = now, UpdatedAt = now },
            new Group { Name = "B", CreatedAt = now, UpdatedAt = now },
            new Group { Name = "C", CreatedAt = now, UpdatedAt = now }
        );

        await db.SaveChangesAsync();
    }
}