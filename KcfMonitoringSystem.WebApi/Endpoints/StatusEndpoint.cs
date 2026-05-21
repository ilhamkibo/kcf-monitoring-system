

using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

public static class StatusEndpoints
{
    public static void MapStatusEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/statuses").WithTags("Statuses");

        group.MapGet("/", async (IStatusService statusService, [FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? search = null, [FromQuery] bool paginate = true, [FromQuery] int? machineId = null, [FromQuery] int? code = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null) =>
        {
            var filter = new StatusFilter
            {
                Page = page,
                Limit = limit,
                Search = search,
                Paginate = paginate,
                MachineId = machineId,
                Code = code,
                StartDate = startDate,
                EndDate = endDate
            };
            var response = await statusService.GetAllAsync(filter);
            return Results.Ok(response);
        }).Produces<ApiPagedResponse<List<StatusDto>>>();

        group.MapGet("/timeline", async (IStatusService statusService, [FromQuery] string? search = null, [FromQuery] int? machineId = null, [FromQuery] int? code = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] int? userId = null, [FromQuery] int? productId = null) =>
        {
            var filter = new StatusFilter
            {
                Search = search,
                MachineId = machineId,
                Code = code,
                StartDate = startDate,
                EndDate = endDate,
                UserId = userId,
                ProductId = productId
            };
            var response = await statusService.GetTimelineAsync(filter);
            return Results.Ok(response);
        }).Produces<ApiResponse<List<StatusTimelineDto>>>();

        group.MapGet("/activity", async (IStatusService statusService, [FromQuery] string? search = null, [FromQuery] int? machineId = null, [FromQuery] int? code = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] int? userId = null, [FromQuery] int? productId = null) =>
        {
            var filter = new StatusFilter
            {
                Search = search,
                MachineId = machineId,
                Code = code,
                StartDate = startDate,
                EndDate = endDate,
                UserId = userId,
                ProductId = productId
            };
            var response = await statusService.GetActivityAsync(filter);
            return Results.Ok(response);
        }).Produces<ApiResponse<List<ActivityDto>>>();
    }
}