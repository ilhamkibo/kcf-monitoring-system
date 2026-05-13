using System;

namespace KcfMonitoringSystem.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string? Role { get; set; } // SPV Produksi, Teknikal, Leader, Operator

    // Foreign Keys
    public int? GroupId { get; set; }
    public Group? Group { get; set; }

    public int? MachineId { get; set; }
    public Machine? Machine { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
