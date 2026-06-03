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

    public async Task<ApiPagedResponse<List<StatusDto>>> GetAllAsync(StatusFilter filter)
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
                x.UpdatedAt?.ToLocalTime(),
                x.Duration
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

        return ApiPagedResponse<List<StatusDto>>.Ok(data, "Success", pagination);
    }

    public async Task<ApiResponse<List<StatusTimelineDto>>> GetTimelineAsync(StatusFilter filter)
    {
        var (start, end) = DateFilterHelper.Normalize(filter.StartDate, filter.EndDate);
        filter.StartDate = start;
        filter.EndDate = end;

        var statuses = await _repository.GetTimelineStatusesAsync(filter);

        var data = statuses
            .GroupBy(s => new { s.MachineId, MachineName = s.Machine.Name })
            .Select(g =>
            {
                var timeline = g.Select(s => new TimelineDto(
                    s.CreatedAt.ToLocalTime(),
                    s.UpdatedAt?.ToLocalTime(),
                    s.Code
                )).ToList();

                if (timeline.Count > 0)
                {
                    var last = timeline[^1];
                    timeline[^1] = new TimelineDto(last.Start, null, last.Status);
                }

                return new StatusTimelineDto(
                    g.Key.MachineId,
                    g.Key.MachineName,
                    timeline
                );
            })
            .ToList();

        return ApiResponse<List<StatusTimelineDto>>.Ok(data, "Success");
    }

    public async Task<ApiResponse<List<ActivityDto>>> GetActivityAsync(StatusFilter filter)
    {
        var (start, end) = DateFilterHelper.Normalize(filter.StartDate, filter.EndDate);
        filter.StartDate = start;
        filter.EndDate = end;

        var statuses = await _repository.GetActivityStatusesAsync(filter);

        var data = statuses
            .GroupBy(s => s.CreatedAt.ToLocalTime().Date)
            .Select(g => new ActivityDto(
                g.Key,
                g.GroupBy(s => new { OperatorName = s.User.Name, ProductName = s.Product.ProductNo, s.Code })
                 .Select(sd => new ActivityDetailDto(
                     sd.Key.OperatorName,
                     sd.Key.ProductName,
                     sd.Sum(s => s.Duration),
                     sd.Key.Code
                 ))
                 .OrderBy(x => x.Operator)
                 .ToList()
            ))
            .OrderByDescending(x => x.Date)
            .ToList();

        return ApiResponse<List<ActivityDto>>.Ok(data, "Success");
    }
}