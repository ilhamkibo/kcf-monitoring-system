namespace KcfMonitoringSystem.Domain.Entities;

public class Status
{
    public int Id { get; set; }
    public int MachineId { get; set; }
    public int Code { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Machine Machine { get; set; } = null!;
}