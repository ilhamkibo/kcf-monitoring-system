namespace KcfMonitoringSystem.Application.Dtos;

public record MachineDto(
    int Id,
    string Name,
    int Order,
    DateTime CreatedAt
);

public record CreateMachineDto(
    string Name,
    int Order
);

public record UpdateMachineDto(
    string Name,
    int Order
);
