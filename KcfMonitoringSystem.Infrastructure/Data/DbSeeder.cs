// using System;
// using KcfMonitoringSystem.Domain.Entities;
// using KcfMonitoringSystem.Infrastructure.Persistence;
// using Microsoft.EntityFrameworkCore;

// namespace KcfMonitoringSystem.Infrastructure.Data;

// public static class DbSeeder
// {
//     public static async Task SeedAsync(AppDbContext db)
//     {
//         // ── Users ──

//     }
// }

using KcfMonitoringSystem.Infrastructure.Persistence;

namespace KcfMonitoringSystem.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await GroupSeeder.SeedAsync(db);
        await MachineSeeder.SeedAsync(db);
        await UserSeeder.SeedAsync(db);
        await ProductSeeder.SeedAsync(db);
        await ProductionSeeder.SeedAsync(db);
        await AlarmHistorySeeder.SeedAsync(db);
    }
}