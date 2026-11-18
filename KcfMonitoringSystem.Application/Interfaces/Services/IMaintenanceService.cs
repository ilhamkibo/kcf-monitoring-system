using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;

namespace KcfMonitoringSystem.Application.Interfaces.Services;

public interface IMaintenanceService
{
    Task<ApiPagedResponse<List<MaintenanceDto>>> GetAllAsync(MaintenanceFilter filter);
}
