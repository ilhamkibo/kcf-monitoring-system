using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Domain.Entities;

namespace KcfMonitoringSystem.Application.Interfaces.Repositories;

public interface IGroupRepository
{
    Task<(List<Group> Data, int TotalCount)> GetAllAsync(GroupFilter filter);
    Task<Group?> GetByIdAsync(int id);
    Task CreateAsync(Group group);
    Task UpdateAsync(int id, Group group);
    Task DeleteAsync(Group group);
}
