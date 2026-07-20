using Microsoft.EntityFrameworkCore;
using KcfMonitoringSystem.Domain.Entities;
using KcfMonitoringSystem.Infrastructure.Persistence;

namespace KcfMonitoringSystem.Infrastructure.Data;

public static class StatusSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Statuses.AnyAsync())
            return;

        var productions = await db.Productions.ToListAsync();
        if (!productions.Any())
            return;

        var now = DateTime.UtcNow;
        var statuses = new List<Status>();
        var statusCodes = new[] { 1, 2, 3, 4, 5 }; // 1=Running, 2=Idle, 3=Error, 4=Maintenance, 5=Stop
        var random = Random.Shared;

        for (int i = 0; i < 30; i++)
        {
            var production = productions[random.Next(productions.Count)];
            var code = statusCodes[random.Next(statusCodes.Length)];
            var createdAt = now.AddMinutes(-random.Next(60, 1440)); // random time in last 24h
            var duration = random.Next(30, 3600); // 30 detik sampai 1 jam

            statuses.Add(new Status
            {
                MachineId = production.MachineId,
                Code = code,
                ProductionId = production.Id,
                Duration = duration,
                CreatedAt = createdAt,
                UpdatedAt = createdAt.AddSeconds(duration)
            });
        }

        db.Statuses.AddRange(statuses);
        await db.SaveChangesAsync();
    }
}