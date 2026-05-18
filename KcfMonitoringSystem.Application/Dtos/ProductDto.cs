namespace KcfMonitoringSystem.Application.Dtos;

public record ProductDto(
    int Id,
    string ProductNo,
    string PartName,
    string PartNo,
    DateTime CreatedAt
);
