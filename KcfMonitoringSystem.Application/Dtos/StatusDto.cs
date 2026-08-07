namespace KcfMonitoringSystem.Application.Dtos;

public record StatusDto(
    int Id,
    int MachineId,
    string MachineName,
    int Code,
    int UserId,
    string UserName,
    int? ProductId,
    string? ProductPartName,
    string? ProductPartNo,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int Duration,
    int Qty
);

public record StatusTimelineDto(
    int MachineId,
    string MachineName,
    List<ProductionTimelineDto> Production
);

public record ProductionTimelineDto(
    string User,
    string? ProductName,
    string? PartNo,
    int Quantity,
    DateTime Start,
    DateTime? End,
    List<SimpleTimelineDto> Timeline
);

public record SimpleTimelineDto(
    DateTime Start,
    DateTime? End,
    int Status,
    string? Message = null,
    int Counter = 0
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