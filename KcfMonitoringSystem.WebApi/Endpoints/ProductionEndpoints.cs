using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace KcfMonitoringSystem.WebApi.Endpoints;

public static class ProductionEndpoints
{
    public static void MapProductionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/productions").WithTags("Productions");

        group.MapGet("/", async (IProductionService productionService, [FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? search = null, [FromQuery] bool paginate = true, [FromQuery] int? machineId = null, [FromQuery] int? userId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] int? partId = null) =>
        {
            var filter = new ProductionFilter
            {
                Page = page,
                Limit = limit,
                Search = search,
                Paginate = paginate,
                MachineId = machineId,
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate,
                PartId = partId
            };
            var response = await productionService.GetAllAsync(filter);
            return Results.Ok(response);
        }).Produces<ApiPagedResponse<List<ProductionDto>>>();
    }
}
