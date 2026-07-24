

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
            new Machine { Name = "SF-200-1", Order = 1, CreatedAt = now, UpdatedAt = now },
            new Machine { Name = "SF-200-2", Order = 2, CreatedAt = now, UpdatedAt = now },
            new Machine { Name = "SF-100-1", Order = 3, CreatedAt = now, UpdatedAt = now },
            new Machine { Name = "SF-100-2", Order = 4, CreatedAt = now, UpdatedAt = now },
            new Machine { Name = "SF-80", Order = 5, CreatedAt = now, UpdatedAt = now },
            new Machine { Name = "SF-50-1", Order = 6, CreatedAt = now, UpdatedAt = now },
            new Machine { Name = "SF-50-2", Order = 7, CreatedAt = now, UpdatedAt = now },
            new Machine { Name = "SF-50-3", Order = 8, CreatedAt = now, UpdatedAt = now },
            new Machine { Name = "JBP-13B7S", Order = 9, CreatedAt = now, UpdatedAt = now }
        );

        await db.SaveChangesAsync();
    }
}