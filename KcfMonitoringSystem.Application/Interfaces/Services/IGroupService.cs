using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;

namespace KcfMonitoringSystem.Application.Interfaces.Services;

public interface IGroupService
{
    Task<ApiPagedResponse<List<GroupDto>>> GetAllAsync(GroupFilter filter);
    Task<ApiResponse<GroupDto>> GetByIdAsync(int id);
    Task<ApiResponse<GroupDto>> CreateAsync(CreateGroupDto createGroupDto);
    Task<ApiResponse<GroupDto>> UpdateAsync(int id, UpdateGroupDto updateGroupDto);
    Task<ApiResponse<object>> DeleteAsync(int id);
}
