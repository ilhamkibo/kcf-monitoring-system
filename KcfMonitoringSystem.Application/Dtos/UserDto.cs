using System;

namespace KcfMonitoringSystem.Application.Dtos;

public record UserDto(
    int Id,
    string Name,
    string? Email,
    string? Username,
    string? Role,
    string? GroupName,
    string? MachineName,
    DateTime CreatedAt
);
