using System;

namespace KcfMonitoringSystem.Application.Dtos;

public record UserDto
(
    int Id,
    string Name,
    string? Email,
    string? Username,
    DateTime CreatedAt
);
