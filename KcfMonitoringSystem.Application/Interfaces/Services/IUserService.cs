using System;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Common;

namespace KcfMonitoringSystem.Application.Services;

public interface IUserService
{
    Task<ApiResponse<List<UserDto>>> GetAllAsync(UserFilter filter);
    Task<ApiResponse<UserDto>> GetByIdAsync(int id);
}