namespace KcfMonitoringSystem.Application.Dtos;

public record DandoriMonthsDto(
    int? Jan,
    int? Feb,
    int? Mar,
    int? Apr,
    int? May,
    int? Jun,
    int? Jul,
    int? Aug,
    int? Sep,
    int? Oct,
    int? Nov,
    int? Dec
);

public record DandoriOperatorDto(
    int Id,
    string Name,
    DandoriMonthsDto Months,
    int? Average
);

public record DandoriAverageDto(
    int? Jan,
    int? Feb,
    int? Mar,
    int? Apr,
    int? May,
    int? Jun,
    int? Jul,
    int? Aug,
    int? Sep,
    int? Oct,
    int? Nov,
    int? Dec,
    int? Overall
);

public record DandoriSummaryDto(
    List<DandoriOperatorDto> Operators,
    DandoriAverageDto Average
);

public record ProductionRecordMachineDto(
    int Id,
    string Code
);

public record ProductionRecordItemDto(
    int Id,
    string No,
    string Name
);

public record ProductionRecordSpeedDto(
    int Minute,
    int Hour
);

public record ProductionRecordOperatorDto(
    int Id,
    string Name
);

public record ProductionRecordTimesDto(
    int Dandori,
    int Running
);

public record ProductionRecordDto(
    string Date,
    ProductionRecordMachineDto Machine,
    ProductionRecordItemDto Item,
    ProductionRecordSpeedDto Speed,
    List<ProductionRecordOperatorDto> Operator,
    ProductionRecordTimesDto Times,
    long ProductQuantity,
    double OperatingRate
);

