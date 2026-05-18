using System;

namespace KcfMonitoringSystem.Application.Dtos;

public record ProductionDto(
    int Id,
    int MachineId,
    string MachineName,
    int UserId,
    string UserName,
    int Quantity,
    DateTime CreatedAt
);
