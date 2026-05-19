using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace KcfMonitoringSystem.WebApi.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", async (IProductService productService, [FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? search = null, [FromQuery] bool paginate = true) =>
        {
            var filter = new ProductFilter { Page = page, Limit = limit, Search = search, Paginate = paginate };
            var response = await productService.GetAllAsync(filter);
            return Results.Ok(response);
        });

        group.MapGet("/{id}", async (int id, IProductService productService) =>
        {
            var response = await productService.GetByIdAsync(id);
            if (!response.Status)
                return Results.NotFound(response);

            return Results.Ok(response);
        });

        group.MapGet("/by-product-no/{productNo}", async (string productNo, IProductService productService) =>
        {
            var response = await productService.GetByProductNoAsync(productNo);
            if (!response.Status)
                return Results.NotFound(response);

            return Results.Ok(response);
        });
    }
}
