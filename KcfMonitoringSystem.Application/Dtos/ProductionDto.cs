namespace KcfMonitoringSystem.Application.Dtos;

public record ProductionDto(
    int Id,
    int MachineId,
    string MachineName,
    int UserId,
    string UserName,
    int? ProductId,
    string? PartNo,
    string? PartName,
    int Quantity,
    DateTime CreatedAt,
    DateTime? UpdatedAt = null
);
