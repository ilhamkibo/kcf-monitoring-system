using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Application.Interfaces.Services;

namespace KcfMonitoringSystem.Application.Services;

public class StatusService : IStatusService
{
    private readonly IStatusRepository _repository;

    public StatusService(IStatusRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<List<StatusDto>>> GetAllAsync(StatusFilter filter)
    {
        var (start, end) = DateFilterHelper.Normalize(filter.StartDate, filter.EndDate);
        filter.StartDate = start;
        filter.EndDate = end;

        var (statuses, totalCount) = await _repository.GetAllAsync(filter);

        var data = statuses.Select(
            x => new StatusDto(
                x.Id,
                x.MachineId,
                x.Machine.Name,
                x.Code,
                x.CreatedAt.ToLocalTime(),
                x.UpdatedAt?.ToLocalTime()
            )
        ).ToList();

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

        return ApiResponse<List<StatusDto>>.Ok(data, "Success", pagination);
    }
}