using System.ComponentModel.DataAnnotations;

namespace KcfMonitoringSystem.Application.Dtos;

public record CreateUserDto(
    [Required(ErrorMessage = "Name is required.")]
    string Name,

    string? Email,
    string? Username,
    string? Role,
    int? GroupId,
    int? MachineId
);
