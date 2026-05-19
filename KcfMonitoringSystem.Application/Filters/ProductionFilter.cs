using System;

namespace KcfMonitoringSystem.Application.Filters;

public class ProductionFilter : BaseFilter
{
    public int? MachineId { get; set; }
    public int? UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? PartId { get; set; }
}
