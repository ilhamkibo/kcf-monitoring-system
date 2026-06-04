using System;

namespace KcfMonitoringSystem.Application.Filters;

public class AlarmHistoryFilter : BaseFilter
{
    public int? MachineId { get; set; }
    public string? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
