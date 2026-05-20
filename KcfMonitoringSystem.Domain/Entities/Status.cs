namespace KcfMonitoringSystem.Domain.Entities;

public class Status
{
    public int Id { get; set; }
    public int MachineId { get; set; }
    public int Code { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int Duration { get; set; }

    // Navigation
    public Machine Machine { get; set; } = null!;
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}