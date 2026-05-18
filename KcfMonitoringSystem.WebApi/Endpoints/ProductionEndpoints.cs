using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace KcfMonitoringSystem.WebApi.Endpoints;

public static class ProductionEndpoints
{
    public static void MapProductionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/production").WithTags("Production");

        group.MapGet("/", async (IProductionService productionService, [FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? search = null, [FromQuery] bool paginate = true, [FromQuery] int? machineId = null, [FromQuery] int? userId = null) =>
        {
            var filter = new ProductionFilter 
            { 
                Page = page, 
                Limit = limit, 
                Search = search, 
                Paginate = paginate,
                MachineId = machineId,
                UserId = userId
            };
            var response = await productionService.GetAllAsync(filter);
            return Results.Ok(response);
        });

        group.MapGet("/{id}", async (int id, IProductionService productionService) =>
        {
            var response = await productionService.GetByIdAsync(id);
            if (!response.Status)
                return Results.NotFound(response);

            return Results.Ok(response);
        });
    }
}
