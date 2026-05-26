using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;

namespace KcfMonitoringSystem.Application.Interfaces.Services;

public interface IMachineService
{
    Task<ApiPagedResponse<List<MachineDto>>> GetAllAsync(MachineFilter filter);
    Task<ApiResponse<MachineDto>> GetByIdAsync(int id);
    Task<ApiResponse<MachineDto>> CreateAsync(CreateMachineDto createMachineDto);
    Task<ApiResponse<MachineDto>> UpdateAsync(UpdateMachineDto updateMachineDto);
    Task<ApiResponse<object>> DeleteAsync(int id);
}