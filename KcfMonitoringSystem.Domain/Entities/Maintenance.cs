namespace KcfMonitoringSystem.Domain.Entities;

public class Maintenance
{
    public int Id { get; set; }
    public int StatusId { get; set; }
    public int? MaintenanceOperatorId { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Status Status { get; set; } = null!;
    public User? MaintenanceOperator { get; set; }
}
