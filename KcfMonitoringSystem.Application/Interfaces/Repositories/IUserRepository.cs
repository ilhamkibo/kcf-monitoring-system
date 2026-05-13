using System;
using KcfMonitoringSystem.Domain.Entities;
using KcfMonitoringSystem.Application.Filters;

namespace KcfMonitoringSystem.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<(List<User> Data, int TotalCount)> GetAllAsync(UserFilter filter);
    Task<User?> GetByIdAsync(int id);
}
