using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Application.Interfaces.Services;
using KcfMonitoringSystem.Domain.Entities;

namespace KcfMonitoringSystem.Application.Services;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;

    public GroupService(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public async Task<ApiPagedResponse<List<GroupDto>>> GetAllAsync(GroupFilter filter)
    {
        var (groups, totalCount) = await _groupRepository.GetAllAsync(filter);
        var data = groups.Select(x => new GroupDto(
            x.Id,
            x.Name,
            x.CreatedAt.ToLocalTime()
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
        return ApiPagedResponse<List<GroupDto>>.Ok(data, "Success", pagination);
    }

    public async Task<ApiResponse<GroupDto>> GetByIdAsync(int id)
    {
        var group = await _groupRepository.GetByIdAsync(id);

        if (group == null)
            return ApiResponse<GroupDto>.Error("Group not found");

        var data = new GroupDto(
            group.Id,
            group.Name,
            group.CreatedAt.ToLocalTime()
        );
        return ApiResponse<GroupDto>.Ok(data);
    }

    public async Task<ApiResponse<GroupDto>> CreateAsync(CreateGroupDto createGroupDto)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(createGroupDto.Name))
        {
            errors.Add("Name", new[] { "Name is required." });
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        var group = new Group
        {
            Name = createGroupDto.Name.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _groupRepository.CreateAsync(group);

        var dto = new GroupDto(
            group.Id,
            group.Name,
            group.CreatedAt.ToLocalTime()
        );

        return ApiResponse<GroupDto>.Ok(dto, "Group created successfully");
    }

    public async Task<ApiResponse<GroupDto>> UpdateAsync(int id, UpdateGroupDto updateGroupDto)
    {
        var group = await _groupRepository.GetByIdAsync(id);
        if (group == null)
            return ApiResponse<GroupDto>.Error("Group not found");

        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(updateGroupDto.Name))
        {
            errors.Add("Name", new[] { "Name is required." });
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        group.Name = updateGroupDto.Name.Trim();
        group.UpdatedAt = DateTime.UtcNow;

        await _groupRepository.UpdateAsync(id, group);

        var dto = new GroupDto(
            group.Id,
            group.Name,
            group.CreatedAt.ToLocalTime()
        );

        return ApiResponse<GroupDto>.Ok(dto, "Group updated successfully");
    }

    public async Task<ApiResponse<object>> DeleteAsync(int id)
    {
        var group = await _groupRepository.GetByIdAsync(id);
        if (group == null)
            return ApiResponse<object>.Error("Group not found");

        await _groupRepository.DeleteAsync(group);
        return ApiResponse<object>.Ok(true, "Group deleted successfully");
    }
}
