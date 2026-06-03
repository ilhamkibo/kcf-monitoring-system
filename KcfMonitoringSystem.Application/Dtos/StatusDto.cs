namespace KcfMonitoringSystem.Application.Dtos;

public record StatusDto(
    int Id,
    int MachineId,
    string MachineName,
    int Code,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int Duration
);

public record StatusTimelineDto(
    int MachineId,
    string MachineName,
    List<TimelineDto> Timeline
);

public record TimelineDto(
    DateTime Start,
    DateTime? End,
    int Status
);

public record ActivityDto(
    DateTime Date,
    List<ActivityDetailDto> Details
);

public record ActivityDetailDto(
    string Operator,
    string Product,
    int TotalTime,
    int Code
);