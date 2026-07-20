namespace KcfMonitoringSystem.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public required string ProductNo { get; set; }
    public required string PartName { get; set; }
    public required string PartNo { get; set; }
    public int? Rpm { get; set; }
    public string? Customer { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<Production> Productions { get; set; } = new List<Production>();
}
