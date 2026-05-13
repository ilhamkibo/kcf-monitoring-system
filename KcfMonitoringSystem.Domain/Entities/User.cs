using System;

namespace KcfMonitoringSystem.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }
    public DateTime CreatedAt { get; set; }
}
