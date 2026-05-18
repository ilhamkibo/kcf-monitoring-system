using KcfMonitoringSystem.Domain.Entities;
using KcfMonitoringSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KcfMonitoringSystem.Infrastructure.Data;

public static class UserSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        var now = DateTime.UtcNow;
        // Load references
        var groupA = await db.Groups.FirstAsync(g => g.Name == "A");
        var groupB = await db.Groups.FirstAsync(g => g.Name == "B");
        var groupC = await db.Groups.FirstAsync(g => g.Name == "C");

        var machines = await db.Machines.ToDictionaryAsync(m => m.Name, m => m);

        db.Users.AddRange(
            // ── Staff (tanpa group & mesin) ──
            new User { Name = "Nana Maulana", Role = "SPV Produksi", CreatedAt = now, UpdatedAt = now },
            new User { Name = "Agung Hermawan", Role = "Teknikal", CreatedAt = now, UpdatedAt = now },

            // ── Group A ──
            new User { Name = "A. Rizal", Role = "Leader", GroupId = groupA.Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "Randy Aprisal. M", Role = "Operator", GroupId = groupA.Id, MachineId = machines["SF-200-1"].Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "Ali Nur. A", Role = "Operator", GroupId = groupA.Id, MachineId = machines["SF-200-2"].Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "Sutiawan", Role = "Operator", GroupId = groupA.Id, MachineId = machines["SF-100-1"].Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "M. Alan. T", Role = "Operator", GroupId = groupA.Id, MachineId = machines["SF-100-2"].Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "Sujasmin", Role = "Operator", GroupId = groupA.Id, MachineId = machines["SF-80"].Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "Andri Lesmana", Role = "Operator", GroupId = groupA.Id, MachineId = machines["SF-50-1"].Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "Yudi Permana", Role = "Operator", GroupId = groupA.Id, MachineId = machines["SF-50-2"].Id, CreatedAt = now, UpdatedAt = now },

            // ── Group B ──
            new User { Name = "Kosasih. S", Role = "Leader", GroupId = groupB.Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "Irfan Sutrisno", Role = "Operator", GroupId = groupB.Id, MachineId = machines["SF-200-1"].Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "Tia Setia", Role = "Operator", GroupId = groupB.Id, MachineId = machines["SF-200-2"].Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "Azis Riyanto", Role = "Operator", GroupId = groupB.Id, MachineId = machines["SF-100-1"].Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "Dadan Arianto", Role = "Operator", GroupId = groupB.Id, MachineId = machines["SF-100-2"].Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "Ardika", Role = "Operator", GroupId = groupB.Id, MachineId = machines["SF-80"].Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "Risqon", Role = "Operator", GroupId = groupB.Id, MachineId = machines["SF-50-1"].Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "Yudi Setiawan", Role = "Operator", GroupId = groupB.Id, MachineId = machines["SF-50-2"].Id, CreatedAt = now, UpdatedAt = now },

            // ── Group C ──
            new User { Name = "M. Hariyanto", Role = "Leader", GroupId = groupC.Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "Rafif", Role = "Operator", GroupId = groupC.Id, MachineId = machines["SF-200-1"].Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "Kendar Lesmana", Role = "Operator", GroupId = groupC.Id, MachineId = machines["SF-200-2"].Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "Arif Hidayat", Role = "Operator", GroupId = groupC.Id, MachineId = machines["SF-100-1"].Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "M. Hamdanita", Role = "Operator", GroupId = groupC.Id, MachineId = machines["SF-100-2"].Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "Dede Rafi", Role = "Operator", GroupId = groupC.Id, MachineId = machines["SF-80"].Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "Ridwan Surya", Role = "Operator", GroupId = groupC.Id, MachineId = machines["SF-50-1"].Id, CreatedAt = now, UpdatedAt = now },
            new User { Name = "Rendi Dwi", Role = "Operator", GroupId = groupC.Id, MachineId = machines["SF-50-2"].Id, CreatedAt = now, UpdatedAt = now }
        );

        await db.SaveChangesAsync();
    }
}