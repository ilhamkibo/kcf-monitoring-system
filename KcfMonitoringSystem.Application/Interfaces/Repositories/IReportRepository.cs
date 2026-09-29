using KcfMonitoringSystem.Domain.Entities;

namespace KcfMonitoringSystem.Application.Interfaces.Repositories;

public interface IReportRepository
{
    Task<(List<User> Operators, List<Status> Statuses)> GetDandoriDataAsync(int year);
    Task<List<Production>> GetProductionRecordsDataAsync(int year, int? machineId = null, int? productId = null);
}
