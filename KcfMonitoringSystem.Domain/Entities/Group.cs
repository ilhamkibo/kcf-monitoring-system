using System;

namespace KcfMonitoringSystem.Domain.Entities;

public class Group
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
}
