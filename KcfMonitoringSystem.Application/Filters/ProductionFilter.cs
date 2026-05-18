using System;

namespace KcfMonitoringSystem.Application.Filters;

public class ProductionFilter : BaseFilter
{
    public int? MachineId { get; set; }
    public int? UserId { get; set; }
}
