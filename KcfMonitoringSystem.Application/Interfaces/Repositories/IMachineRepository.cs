using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Domain.Entities;

namespace KcfMonitoringSystem.Application.Interfaces.Repositories;

public interface IMachineRepository
{
    Task<(List<Machine> Data, int TotalCount)> GetAllAsync(MachineFilter filter);
    Task<Machine?> GetByIdAsync(int id);
    Task CreateAsync(Machine machine);
    Task UpdateAsync(Machine machine);
    Task DeleteAsync(Machine machine);
}