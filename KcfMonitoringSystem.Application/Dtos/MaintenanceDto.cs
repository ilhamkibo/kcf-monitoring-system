namespace KcfMonitoringSystem.Application.Dtos;

public record MaintenanceDto(
    int Id,
    int MachineId,
    string MachineName,
    int Code,
    int UserId,
    string UserName,
    int? ProductId,
    string? ProductPartName,
    string? ProductPartNo,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int Duration,
    long Qty,
    List<MaintenanceDetailDto> MaintenanceDetails
);

public record MaintenanceDetailDto(
    int Id,
    int? MaintenanceOperatorId,
    string? MaintenanceOperatorName,
    string? Reason,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
