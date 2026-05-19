using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;

namespace KcfMonitoringSystem.Application.Interfaces.Services;

public interface IStatusService
{
    Task<ApiResponse<List<StatusDto>>> GetAllAsync(StatusFilter filter);
}