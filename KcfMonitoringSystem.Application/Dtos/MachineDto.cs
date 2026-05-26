namespace KcfMonitoringSystem.Application.Dtos;

public record MachineDto(
    int Id,
    string Name,
    DateTime CreatedAt
);

public record CreateMachineDto(
    string Name
);

public record UpdateMachineDto(
    string Name
);