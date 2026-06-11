using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Application.Interfaces.Services;
using KcfMonitoringSystem.Domain.Entities;

namespace KcfMonitoringSystem.Application.Services;

public class MachineService : IMachineService
{
    private readonly IMachineRepository _machineRepository;

    public MachineService(IMachineRepository machineRepository)
    {
        _machineRepository = machineRepository;
    }

    public async Task<ApiPagedResponse<List<MachineDto>>> GetAllAsync(MachineFilter filter)
    {
        var (machines, totalCount) = await _machineRepository.GetAllAsync(filter);
        var data = machines.Select(x => new MachineDto(
            x.Id,
            x.Name,
            x.CreatedAt
        )).ToList();

        PaginationMetadata? pagination = null;
        if (filter.Paginate == true)
        {
            pagination = new PaginationMetadata
            {
                Page = filter.Page,
                Limit = filter.Limit,
                Total = totalCount,
                TotalPages = filter.Limit > 0 ? (int)Math.Ceiling((double)totalCount / filter.Limit) : 0
            };
        }
        return ApiPagedResponse<List<MachineDto>>.Ok(data, "Success", pagination);
    }

    public async Task<ApiResponse<MachineDto>> GetByIdAsync(int id)
    {
        var machine = await _machineRepository.GetByIdAsync(id);

        if (machine == null)
            return ApiResponse<MachineDto>.Error("Machine not found");

        var data = new MachineDto(
            machine.Id,
            machine.Name,
            machine.CreatedAt
        );
        return ApiResponse<MachineDto>.Ok(data);
    }

    public async Task<ApiResponse<MachineDto>> CreateAsync(CreateMachineDto createMachineDto)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(createMachineDto.Name))
        {
            errors.Add("Name", new[] { "Name is required." });
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        var machine = new Machine
        {
            Name = createMachineDto.Name.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _machineRepository.CreateAsync(machine);

        var dto = new MachineDto(
            machine.Id,
            machine.Name,
            machine.CreatedAt
        );

        return ApiResponse<MachineDto>.Ok(dto, "Machine created successfully");
    }

    public async Task<ApiResponse<MachineDto>> UpdateAsync(int id, UpdateMachineDto updateMachineDto)
    {
        var machine = await _machineRepository.GetByIdAsync(id);
        if (machine == null)
            return ApiResponse<MachineDto>.Error("Machine not found");

        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(updateMachineDto.Name))
        {
            errors.Add("Name", new[] { "Name is required." });
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        machine.Name = updateMachineDto.Name.Trim();
        machine.UpdatedAt = DateTime.UtcNow;

        await _machineRepository.UpdateAsync(id, machine);

        var dto = new MachineDto(
            machine.Id,
            machine.Name,
            machine.CreatedAt
        );

        return ApiResponse<MachineDto>.Ok(dto, "Machine updated successfully");
    }

    public async Task<ApiResponse<object>> DeleteAsync(int id)
    {
        var machine = await _machineRepository.GetByIdAsync(id);
        if (machine == null)
            return ApiResponse<object>.Error("Machine not found");

        await _machineRepository.DeleteAsync(machine);
        return ApiResponse<object>.Ok(true, "Machine deleted successfully");
    }
}