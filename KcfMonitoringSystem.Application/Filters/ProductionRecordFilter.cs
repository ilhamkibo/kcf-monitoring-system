namespace KcfMonitoringSystem.Application.Filters;

public class ProductionRecordFilter : BaseFilter
{
    public int? Year { get; set; }
    public int? MachineId { get; set; }
    public int? ProductId { get; set; }
}
