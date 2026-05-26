using System.ComponentModel.DataAnnotations;

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

public record CreateUserDto(
    [Required(ErrorMessage = "Name is required.")]
    string Name,

    string? Email,
    string? Username,
    string? Role,
    int? GroupId,
    int? MachineId
);
