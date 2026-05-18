namespace KcfMonitoringSystem.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public required string ProductNo { get; set; }
    public required string PartName { get; set; }
    public required string PartNo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
