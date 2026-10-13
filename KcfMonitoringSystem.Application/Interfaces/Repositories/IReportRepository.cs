using KcfMonitoringSystem.Domain.Entities;

namespace KcfMonitoringSystem.Application.Interfaces.Repositories;

public interface IReportRepository
{
    Task<List<User>> GetOperatorsAsync();
    Task<List<Production>> GetProductionRecordsDataAsync(int year, int? machineId = null);
}
