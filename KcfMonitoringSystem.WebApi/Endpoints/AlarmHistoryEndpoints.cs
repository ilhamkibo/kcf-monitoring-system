using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

public static class AlarmHistoryEndpoints
{
    public static void MapAlarmHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/alarm-histories").WithTags("Alarm Histories");

        group.MapGet("/", async (
            IAlarmHistoryService alarmHistoryService,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string? search = null,
            [FromQuery] bool paginate = true,
            [FromQuery] int? machineId = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null) =>
        {
            var filter = new AlarmHistoryFilter
            {
                Page = page,
                Limit = limit,
                Search = search,
                Paginate = paginate,
                MachineId = machineId,
                Status = status,
                StartDate = startDate,
                EndDate = endDate
            };
            var response = await alarmHistoryService.GetAllAsync(filter);
            return Results.Ok(response);
        }).Produces<ApiPagedResponse<List<AlarmHistoryDto>>>();
    }
}
