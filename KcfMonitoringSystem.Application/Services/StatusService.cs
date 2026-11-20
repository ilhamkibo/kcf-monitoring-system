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

        var hasDateRange = StatusRangeClipper.HasDateRange(filter.StartDate, filter.EndDate);
        var data = statuses.Select(x =>
        {
            var period = hasDateRange
                ? StatusRangeClipper.Clip(x, filter.StartDate, filter.EndDate)
                : new ClippedStatusPeriod(x.CreatedAt, x.UpdatedAt, x.Duration, x.UpdatedAt ?? x.CreatedAt);

            return new StatusDto(
                x.Id,
                x.MachineId,
                x.Machine.Name,
                x.Code,
                x.Production.UserId,
                x.Production.User.Name,
                x.Production.ProductId,
                x.Production.Product?.PartName,
                x.Production.Product?.PartNo,
                period.Start,
                period.End,
                period.Duration,
                x.Qty
            );
        }).ToList();

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

        var data = BuildTimelineData(statuses, filter);

        return ApiResponse<List<StatusTimelineDto>>.Ok(data, "Success");
    }

    public async Task<ApiResponse<List<ActivityDto>>> GetActivityAsync(StatusFilter filter)
    {
        var (start, end) = DateFilterHelper.Normalize(filter.StartDate, filter.EndDate);
        filter.StartDate = start;
        filter.EndDate = end;

        var statuses = await _repository.GetActivityStatusesAsync(filter);

        var hasDateRange = StatusRangeClipper.HasDateRange(filter.StartDate, filter.EndDate);
        var activitySegments = hasDateRange
            ? statuses.SelectMany(s => CreateDailyActivitySegments(s, filter.StartDate, filter.EndDate))
            : statuses.Select(s => new ActivitySegment(
                s.CreatedAt.Date,
                s.Production.User.Name,
                s.Production.Product?.ProductNo ?? string.Empty,
                s.Code,
                s.Duration));

        var data = activitySegments
            .GroupBy(s => s.Date)
            .Select(g => new ActivityDto(
                g.Key,
                g.GroupBy(s => new { s.OperatorName, s.ProductName, s.Code })
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

    public async Task<ApiResponse<List<StatusTimelineDto>>> GetLatestProductTimelineAsync(StatusFilter filter)
    {
        var (start, end) = DateFilterHelper.Normalize(filter.StartDate, filter.EndDate);
        filter.StartDate = start;
        filter.EndDate = end;

        var statuses = await _repository.GetLatestProductStatusesAsync(filter);

        var data = BuildTimelineData(statuses, filter);

        return ApiResponse<List<StatusTimelineDto>>.Ok(data, "Success");
    }

    private static List<StatusTimelineDto> BuildTimelineData(
        List<KcfMonitoringSystem.Domain.Entities.Status> statuses,
        StatusFilter filter)
    {
        var today = DateTime.Now.Date;
        var isToday = (!filter.StartDate.HasValue || filter.StartDate.Value.Date <= today) &&
                      (!filter.EndDate.HasValue || filter.EndDate.Value.Date > today);
        var hasDateRange = StatusRangeClipper.HasDateRange(filter.StartDate, filter.EndDate);

        return statuses
            .GroupBy(s => new { s.MachineId, MachineName = s.Machine.Name })
            .Select(machineGroup =>
            {
                var latestStatusId = machineGroup.Max(s => s.Id);
                var productions = machineGroup
                    .GroupBy(s => new
                    {
                        UserName = s.Production.User.Name,
                        ProductName = s.Production.Product?.PartName,
                        s.Production.Product?.PartNo,
                        Quantity = s.Production.ActualQty,
                        s.Production.CreatedAt,
                        s.Production.UpdatedAt,
                    })
                    .Select(productionGroup =>
                    {
                        var timeline = productionGroup.Select(status =>
                        {
                            var period = hasDateRange
                                ? StatusRangeClipper.Clip(status, filter.StartDate, filter.EndDate)
                                : new ClippedStatusPeriod(
                                    status.CreatedAt,
                                    status.UpdatedAt,
                                    status.Duration,
                                    status.UpdatedAt ?? status.CreatedAt);

                            var end = isToday && status.Id == latestStatusId
                                ? null
                                : period.End;

                            return new SimpleTimelineDto(
                                period.Start,
                                end,
                                status.Code,
                                status.Code == 2
                                    ? status.AlarmHistories.OrderBy(a => a.Id).FirstOrDefault()?.Message ?? ""
                                    : "",
                                status.Qty,
                                period.Duration
                            );
                        }).ToList();

                        return new ProductionTimelineDto(
                            productionGroup.Key.UserName,
                            productionGroup.Key.ProductName,
                            productionGroup.Key.PartNo,
                            productionGroup.Key.Quantity,
                            productionGroup.Key.CreatedAt,
                            productionGroup.Key.UpdatedAt,
                            timeline
                        );
                    })
                    .ToList();

                return new StatusTimelineDto(
                    machineGroup.Key.MachineId,
                    machineGroup.Key.MachineName,
                    productions
                );
            })
            .ToList();
    }

    private static IEnumerable<ActivitySegment> CreateDailyActivitySegments(
        KcfMonitoringSystem.Domain.Entities.Status status,
        DateTime? startDate,
        DateTime? endDate)
    {
        var period = StatusRangeClipper.Clip(status, startDate, endDate);
        var segmentStart = period.Start;
        var effectiveEnd = period.EffectiveEnd;

        while (segmentStart < effectiveEnd)
        {
            var nextDay = segmentStart.Date.AddDays(1);
            var segmentEnd = effectiveEnd < nextDay ? effectiveEnd : nextDay;
            var duration = (int)Math.Max(0, Math.Floor((segmentEnd - segmentStart).TotalSeconds));

            if (duration > 0)
            {
                yield return new ActivitySegment(
                    segmentStart.Date,
                    status.Production.User.Name,
                    status.Production.Product?.ProductNo ?? string.Empty,
                    status.Code,
                    duration);
            }

            segmentStart = segmentEnd;
        }
    }

    private sealed record ActivitySegment(
        DateTime Date,
        string OperatorName,
        string ProductName,
        int Code,
        int Duration);
}
