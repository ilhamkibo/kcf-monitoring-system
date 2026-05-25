using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Domain.Entities;

namespace KcfMonitoringSystem.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiPagedResponse<List<UserDto>>> GetAllAsync(UserFilter filter)
    {
        var (users, totalCount) = await _repository.GetAllAsync(filter);

        var data = users.Select(x => new UserDto(
            x.Id,
            x.Name,
            x.Email,
            x.Username,
            x.Role,
            x.Group?.Name,
            x.Machine?.Name,
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

        return ApiPagedResponse<List<UserDto>>.Ok(data, "Success", pagination);
    }

    public async Task<ApiResponse<UserDto>> GetByIdAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);

        if (user == null)
            return ApiResponse<UserDto>.Error("User not found");

        var data = new UserDto(
            user.Id,
            user.Name,
            user.Email,
            user.Username,
            user.Role,
            user.Group?.Name,
            user.Machine?.Name,
            user.CreatedAt.ToLocalTime()
        );

        return ApiResponse<UserDto>.Ok(data);
    }

    public async Task<ApiResponse<UserDto>> CreateAsync(CreateUserDto createUserDto)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(createUserDto.Name))
        {
            errors.Add("Name", new[] { "Name is required." });
        }

        if (!string.IsNullOrWhiteSpace(createUserDto.Username))
        {
            var usernameExists = await _repository.UsernameExistsAsync(createUserDto.Username);
            if (usernameExists)
            {
                errors.Add("Username", new[] { "Username already exists." });
            }
        }

        if (createUserDto.GroupId.HasValue)
        {
            var groupExists = await _repository.GroupExistsAsync(createUserDto.GroupId.Value);
            if (!groupExists)
            {
                errors.Add("GroupId", new[] { "Group not found." });
            }
        }

        if (createUserDto.MachineId.HasValue)
        {
            var machineExists = await _repository.MachineExistsAsync(createUserDto.MachineId.Value);
            if (!machineExists)
            {
                errors.Add("MachineId", new[] { "Machine not found." });
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        var user = new User
        {
            Name = createUserDto.Name.Trim(),
            Email = createUserDto.Email?.Trim(),
            Username = createUserDto.Username?.Trim(),
            Role = createUserDto.Role?.Trim(),
            GroupId = createUserDto.GroupId,
            MachineId = createUserDto.MachineId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(user);

        var createdUser = await _repository.GetByIdAsync(user.Id);
        if (createdUser == null)
        {
            return ApiResponse<UserDto>.Error("Failed to retrieve created user");
        }

        var dto = new UserDto(
            createdUser.Id,
            createdUser.Name,
            createdUser.Email,
            createdUser.Username,
            createdUser.Role,
            createdUser.Group?.Name,
            createdUser.Machine?.Name,
            createdUser.CreatedAt.ToLocalTime()
        );

        return ApiResponse<UserDto>.Ok(dto, "User created successfully");
    }

    public async Task<ApiResponse<object>> DeleteAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null)
        {
            return ApiResponse<object>.Error("User not found");
        }

        await _repository.DeleteAsync(user);

        return ApiResponse<object>.Ok(true, "User deleted successfully");
    }
}
