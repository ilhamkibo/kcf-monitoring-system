using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
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
        }).Produces<ApiPagedResponse<List<UserDto>>>();

        group.MapGet("/{id}", async (int id, IUserService userService) =>
        {
            var response = await userService.GetByIdAsync(id);
            if (!response.Status)
                return Results.NotFound(ApiErrorResponse.Create(response.Message));

            return Results.Ok(response);
        }).Produces<ApiResponse<UserDto>>()
          .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/", async ([FromBody] CreateUserDto createUserDto, IUserService userService) =>
        {
            var response = await userService.CreateAsync(createUserDto);
            if (!response.Status)
                return Results.BadRequest(ApiErrorResponse.Create(response.Message));

            return Results.Created($"/api/users/{response.Data.Id}", response);
        }).Produces<ApiResponse<UserDto>>(StatusCodes.Status201Created)
          .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id}", async (int id, IUserService userService) =>
        {
            var response = await userService.DeleteAsync(id);
            if (!response.Status)
                return Results.NotFound(ApiErrorResponse.Create(response.Message));

            return Results.Ok(response);
        }).Produces<ApiResponse<object>>()
          .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
    }
}
