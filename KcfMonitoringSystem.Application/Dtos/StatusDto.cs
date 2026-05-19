namespace KcfMonitoringSystem.Application.Dtos;

public record StatusDto(
    int Id,
    int MachineId,
    string MachineName,
    int Code,
    DateTime CreatedAt,
    DateTime UpdatedAt
);