using KcfMonitoringSystem.Domain.Entities;
using KcfMonitoringSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KcfMonitoringSystem.Infrastructure.Data;

public static class AlarmHistorySeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.AlarmHistories.AnyAsync())
            return;

        var machines = await db.Machines.ToListAsync();
        if (!machines.Any())
            return;

        var now = DateTime.UtcNow;
        var alarmHistories = new List<AlarmHistory>();

        foreach (var machine in machines)
        {
            // Case 1: Resolved alarms (triggered and recovered)
            for (int i = 1; i <= 5; i++)
            {
                var triggerTime = now.AddDays(-i).AddHours(-i);
                var recoverTime = triggerTime.AddMinutes(30 + i * 10);
                
                alarmHistories.Add(new AlarmHistory
                {
                    MachineId = machine.Id,
                    Status = "recovered",
                    TriggerTime = triggerTime,
                    RecoverTime = recoverTime,
                    Message = $"Alarm for {machine.Name} - Resolved case {i}",
                    Timestamp = recoverTime,
                    CreatedAt = triggerTime,
                    UpdatedAt = recoverTime
                });
            }

            // Case 2: Active alarms (triggered but not recovered) - only for some machines
            if (machine.Id % 3 == 0)
            {
                var triggerTime = now.AddMinutes(-45);
                alarmHistories.Add(new AlarmHistory
                {
                    MachineId = machine.Id,
                    Status = "triggered",
                    TriggerTime = triggerTime,
                    RecoverTime = null,
                    Message = $"Active Alarm for {machine.Name} - Critical state",
                    Timestamp = triggerTime,
                    CreatedAt = triggerTime,
                    UpdatedAt = triggerTime
                });
            }
        }

        // Case 3: Bulk alarms for a specific machine to test load/pagination
        var bulkMachine = machines.First();
        for (int i = 0; i < 20; i++)
        {
            var triggerTime = now.AddDays(-10).AddHours(-i);
            var recoverTime = triggerTime.AddMinutes(15);
            
            alarmHistories.Add(new AlarmHistory
            {
                MachineId = bulkMachine.Id,
                Status = "recovered",
                TriggerTime = triggerTime,
                RecoverTime = recoverTime,
                Message = $"Bulk History {i} for {bulkMachine.Name}",
                Timestamp = recoverTime,
                CreatedAt = triggerTime,
                UpdatedAt = recoverTime
            });
        }

        db.AlarmHistories.AddRange(alarmHistories);
        await db.SaveChangesAsync();
    }
}
