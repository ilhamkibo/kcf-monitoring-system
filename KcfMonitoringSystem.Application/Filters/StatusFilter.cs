namespace KcfMonitoringSystem.Application.Filters;

public class StatusFilter : BaseFilter
{
    public int? MachineId { get; set; }
    public int? Code { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}