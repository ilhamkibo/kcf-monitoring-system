using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Domain.Entities;

namespace KcfMonitoringSystem.Application.Interfaces.Repositories;

public interface IMaintenanceRepository
{
    Task<(List<Status> Data, int TotalCount)> GetAllAsync(MaintenanceFilter filter);
}
