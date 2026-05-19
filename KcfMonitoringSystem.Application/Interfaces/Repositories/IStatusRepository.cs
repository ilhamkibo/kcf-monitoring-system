using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Domain.Entities;

namespace KcfMonitoringSystem.Application.Interfaces.Repositories;

public interface IStatusRepository
{
    Task<(List<Status> Data, int TotalCount)> GetAllAsync(StatusFilter filter);
}