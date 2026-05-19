using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace KcfMonitoringSystem.WebApi.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapGet("/", async (IUserService userService, [FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? search = null, [FromQuery] bool paginate = true) =>
        {
            var filter = new UserFilter { Page = page, Limit = limit, Search = search, Paginate = paginate };
            var response = await userService.GetAllAsync(filter);
            return Results.Ok(response);
        });

        group.MapGet("/{id}", async (int id, IUserService userService) =>
        {
            var response = await userService.GetByIdAsync(id);
            if (!response.Status)
                return Results.NotFound(response);

            return Results.Ok(response);
        });
    }
}
