using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace KcfMonitoringSystem.WebApi.Endpoints;

public static class MachineEndpoints
{
    public static void MapMachineEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/machines").WithTags("Machines");

        group.MapGet("/", async (IMachineService machineService, [FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? search = null, [FromQuery] bool paginate = true) =>
        {
            var filter = new MachineFilter { Page = page, Limit = limit, Search = search, Paginate = paginate };
            var response = await machineService.GetAllAsync(filter);
            return Results.Ok(response);
        }).Produces<ApiPagedResponse<List<MachineDto>>>();

        group.MapGet("/{id}", async (int id, IMachineService machineService) =>
        {
            var response = await machineService.GetByIdAsync(id);
            if (!response.Status)
                return Results.NotFound(ApiErrorResponse.Create(response.Message));

            return Results.Ok(response);
        }).Produces<ApiResponse<MachineDto>>()
          .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/", async ([FromBody] CreateMachineDto createMachineDto, IMachineService machineService) =>
        {
            var response = await machineService.CreateAsync(createMachineDto);
            if (!response.Status)
                return Results.BadRequest(ApiErrorResponse.Create(response.Message));

            return Results.Created($"/api/machines/{response.Data.Id}", response);
        }).Produces<ApiResponse<MachineDto>>(StatusCodes.Status201Created)
          .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id}", async (int id, [FromBody] UpdateMachineDto updateMachineDto, IMachineService machineService) =>
        {
            if (id != updateMachineDto.Id)
                return Results.BadRequest(ApiErrorResponse.Create("ID in path does not match ID in body."));

            var response = await machineService.UpdateAsync(updateMachineDto);
            if (!response.Status)
                return Results.NotFound(ApiErrorResponse.Create(response.Message));

            return Results.Ok(response);
        }).Produces<ApiResponse<MachineDto>>()
          .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
          .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id}", async (int id, IMachineService machineService) =>
        {
            var response = await machineService.DeleteAsync(id);
            if (!response.Status)
                return Results.NotFound(ApiErrorResponse.Create(response.Message));

            return Results.Ok(response);
        }).Produces<ApiResponse<object>>()
          .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
    }
}
