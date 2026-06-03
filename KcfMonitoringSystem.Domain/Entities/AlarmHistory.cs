using System;

namespace KcfMonitoringSystem.Domain.Entities;

public class AlarmHistory
{
    public int Id { get; set; }
    public int MachineId { get; set; }
    public required string Status { get; set; } // "triggered" or "recovered"
    public DateTime TriggerTime { get; set; }
    public DateTime? RecoverTime { get; set; }
    public required string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Machine Machine { get; set; } = null!;
}
