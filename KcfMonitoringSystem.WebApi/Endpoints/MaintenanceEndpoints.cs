using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace KcfMonitoringSystem.WebApi.Endpoints;

public static class MaintenanceEndpoints
{
    public static void MapMaintenanceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/maintenance").WithTags("Maintenance");

        group.MapGet("/", async (
            IMaintenanceService maintenanceService,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string? search = null,
            [FromQuery] bool paginate = true,
            [FromQuery] int? machineId = null,
            [FromQuery] int? code = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null) =>
        {
            var filter = new MaintenanceFilter
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

            var response = await maintenanceService.GetAllAsync(filter);
            return Results.Ok(response);
        }).Produces<ApiPagedResponse<List<MaintenanceDto>>>();
    }
}
