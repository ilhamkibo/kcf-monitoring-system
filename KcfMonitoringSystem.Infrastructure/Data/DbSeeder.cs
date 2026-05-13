using System;
using KcfMonitoringSystem.Domain.Entities;
using KcfMonitoringSystem.Infrastructure.Persistence;

namespace KcfMonitoringSystem.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (db.Users.Any()) return;

        db.Users.AddRange(
            new User { Name = "Ilham Smith", Email = "ilham@example.com", Username = "ilhamsmith", CreatedAt = DateTime.UtcNow },
            new User { Name = "Budi Johnson", Email = "budi@example.com", Username = "budijohnson", CreatedAt = DateTime.UtcNow }
        );

        await db.SaveChangesAsync();
    }
}