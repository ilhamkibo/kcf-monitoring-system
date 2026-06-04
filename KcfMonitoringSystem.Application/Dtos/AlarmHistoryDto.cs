using System;

namespace KcfMonitoringSystem.Application.Dtos;

public record AlarmHistoryDto(
    int Id,
    int MachineId,
    string MachineName,
    string Status,
    DateTime TriggerTime,
    DateTime? RecoverTime,
    string Message,
    DateTime Timestamp,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
