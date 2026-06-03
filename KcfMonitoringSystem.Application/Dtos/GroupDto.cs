namespace KcfMonitoringSystem.Application.Dtos;

public record GroupDto(
    int Id,
    string Name,
    DateTime CreatedAt
);

public record CreateGroupDto(
    string Name
);

public record UpdateGroupDto(
    string Name
);
