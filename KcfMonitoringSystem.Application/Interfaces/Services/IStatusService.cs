using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;

namespace KcfMonitoringSystem.Application.Interfaces.Services;

public interface IStatusService
{
    Task<ApiResponse<List<StatusDto>>> GetAllAsync(StatusFilter filter);
    Task<ApiResponse<List<StatusTimelineDto>>> GetTimelineAsync(StatusFilter filter);
    Task<ApiResponse<List<ActivityDto>>> GetActivityAsync(StatusFilter filter);
}