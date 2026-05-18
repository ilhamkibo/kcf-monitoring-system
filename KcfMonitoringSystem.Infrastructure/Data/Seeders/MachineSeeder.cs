

using KcfMonitoringSystem.Domain.Entities;
using KcfMonitoringSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KcfMonitoringSystem.Infrastructure.Data;

public static class MachineSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Machines.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        db.Machines.AddRange(
            new Machine { Name = "SF-200-1", CreatedAt = now, UpdatedAt = now },
            new Machine { Name = "SF-200-2", CreatedAt = now, UpdatedAt = now },
            new Machine { Name = "SF-100-1", CreatedAt = now, UpdatedAt = now },
            new Machine { Name = "SF-100-2", CreatedAt = now, UpdatedAt = now },
            new Machine { Name = "SF-80", CreatedAt = now, UpdatedAt = now },
            new Machine { Name = "SF-50-1", CreatedAt = now, UpdatedAt = now },
            new Machine { Name = "SF-50-2", CreatedAt = now, UpdatedAt = now }
        );

        await db.SaveChangesAsync();
    }
}