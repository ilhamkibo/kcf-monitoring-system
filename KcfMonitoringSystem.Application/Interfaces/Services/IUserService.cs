using System;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Common;

namespace KcfMonitoringSystem.Application.Services;

public interface IUserService
{
    Task<ApiPagedResponse<List<UserDto>>> GetAllAsync(UserFilter filter);
    Task<ApiResponse<UserDto>> GetByIdAsync(int id);
    Task<ApiResponse<UserDto>> CreateAsync(CreateUserDto createUserDto);
    Task<ApiResponse<object>> DeleteAsync(int id);
}
