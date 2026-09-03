using System;
using KcfMonitoringSystem.Domain.Entities;
using KcfMonitoringSystem.Application.Filters;

namespace KcfMonitoringSystem.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<(List<User> Data, int TotalCount)> GetAllAsync(UserFilter filter);
    Task<(List<User> Data, int TotalCount)> GetAllDeletedAsync(UserFilter filter);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetDeletedByIdAsync(int id);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task RestoreAsync(User user);
    Task<bool> GroupExistsAsync(int groupId);
    Task<bool> MachineExistsAsync(int machineId);
    Task<bool> UsernameExistsAsync(string username, int? excludeId = null);
}
