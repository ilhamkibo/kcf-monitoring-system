using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;

namespace KcfMonitoringSystem.Application.Interfaces.Services;

public interface IAlarmHistoryService
{
    Task<ApiPagedResponse<List<AlarmHistoryDto>>> GetAllAsync(AlarmHistoryFilter filter);
}
