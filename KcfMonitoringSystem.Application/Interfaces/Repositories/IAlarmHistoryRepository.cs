using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Domain.Entities;

namespace KcfMonitoringSystem.Application.Interfaces.Repositories;

public interface IAlarmHistoryRepository
{
    Task<(List<AlarmHistory> Data, int TotalCount)> GetAllAsync(AlarmHistoryFilter filter);
}
