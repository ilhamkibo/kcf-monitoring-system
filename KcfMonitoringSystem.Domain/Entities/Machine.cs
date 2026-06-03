using System;

namespace KcfMonitoringSystem.Domain.Entities;

public class Machine
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Production> Productions { get; set; } = new List<Production>();
    public ICollection<Status> Statuses { get; set; } = new List<Status>();
    public ICollection<AlarmHistory> AlarmHistories { get; set; } = new List<AlarmHistory>();
}
