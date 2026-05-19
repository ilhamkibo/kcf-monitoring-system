using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Repositories;

namespace KcfMonitoringSystem.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<List<UserDto>>> GetAllAsync(UserFilter filter)
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

        return ApiResponse<List<UserDto>>.Ok(data, "Success", pagination);
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
}
