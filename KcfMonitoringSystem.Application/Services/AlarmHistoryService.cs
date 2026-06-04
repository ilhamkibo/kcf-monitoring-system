using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Application.Interfaces.Services;

namespace KcfMonitoringSystem.Application.Services;

public class AlarmHistoryService : IAlarmHistoryService
{
    private readonly IAlarmHistoryRepository _repository;

    public AlarmHistoryService(IAlarmHistoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiPagedResponse<List<AlarmHistoryDto>>> GetAllAsync(AlarmHistoryFilter filter)
    {
        var (start, end) = DateFilterHelper.Normalize(filter.StartDate, filter.EndDate);
        filter.StartDate = start;
        filter.EndDate = end;

        var (alarmHistories, totalCount) = await _repository.GetAllAsync(filter);

        var data = alarmHistories.Select(x => new AlarmHistoryDto(
            x.Id,
            x.MachineId,
            x.Machine.Name,
            x.Status,
            x.TriggerTime.ToLocalTime(),
            x.RecoverTime?.ToLocalTime(),
            x.Message,
            x.Timestamp.ToLocalTime(),
            x.CreatedAt.ToLocalTime(),
            x.UpdatedAt.ToLocalTime()
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

        return ApiPagedResponse<List<AlarmHistoryDto>>.Ok(data, "Success", pagination);
    }
}
