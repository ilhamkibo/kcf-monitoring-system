using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace KcfMonitoringSystem.WebApi.Endpoints;

public static class GroupEndpoints
{
    public static void MapGroupEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/groups").WithTags("Groups");

        group.MapGet("/", async (IGroupService groupService, [FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? search = null, [FromQuery] bool paginate = true) =>
        {
            var filter = new GroupFilter { Page = page, Limit = limit, Search = search, Paginate = paginate };
            var response = await groupService.GetAllAsync(filter);
            return Results.Ok(response);
        }).Produces<ApiPagedResponse<List<GroupDto>>>();

        group.MapGet("/{id}", async (int id, IGroupService groupService) =>
        {
            var response = await groupService.GetByIdAsync(id);
            if (!response.Status)
                return Results.NotFound(ApiErrorResponse.Create(response.Message));

            return Results.Ok(response);
        }).Produces<ApiResponse<GroupDto>>()
          .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/", async ([FromBody] CreateGroupDto createGroupDto, IGroupService groupService) =>
        {
            var response = await groupService.CreateAsync(createGroupDto);
            if (!response.Status)
                return Results.BadRequest(ApiErrorResponse.Create(response.Message));

            return Results.Created($"/api/groups/{response.Data.Id}", response);
        }).Produces<ApiResponse<GroupDto>>(StatusCodes.Status201Created)
          .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id}", async (int id, [FromBody] UpdateGroupDto updateGroupDto, IGroupService groupService) =>
        {
            var response = await groupService.UpdateAsync(id, updateGroupDto);
            if (!response.Status)
                return Results.NotFound(ApiErrorResponse.Create(response.Message));

            return Results.Ok(response);
        }).Produces<ApiResponse<GroupDto>>()
          .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id}", async (int id, IGroupService groupService) =>
        {
            var response = await groupService.DeleteAsync(id);
            if (!response.Status)
                return Results.NotFound(ApiErrorResponse.Create(response.Message));

            return Results.Ok(response);
        }).Produces<ApiResponse<object>>()
          .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
    }
}
