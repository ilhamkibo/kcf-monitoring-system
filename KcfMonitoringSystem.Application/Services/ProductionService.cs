using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Repositories;

namespace KcfMonitoringSystem.Application.Services;

public class ProductionService : IProductionService
{
    private readonly IProductionRepository _repository;

    public ProductionService(IProductionRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<List<ProductionDto>>> GetAllAsync(ProductionFilter filter)
    {
        NormalizeDateFilter(filter);
        var (productions, totalCount) = await _repository.GetAllAsync(filter);

        var data = productions.Select(x => new ProductionDto(
            x.Id,
            x.MachineId,
            x.Machine?.Name ?? "",
            x.UserId,
            x.User?.Name ?? "",
            x.ProductId,
            x.Product?.ProductNo,
            x.Product?.PartName,
            x.Quantity,
            x.CreatedAt.ToLocalTime()
        )).ToList();

        PaginationMetadata? pagination = null;
        if (filter.Paginate == true)
        {
            pagination = new PaginationMetadata
            {
                Page = filter.Page,
                Limit = filter.Limit,
                Total = totalCount,
                TotalPages = filter.Limit > 0 ? (int)Math.Ceiling((double)totalCount / filter.Limit) : 0
            };
        }

        return ApiResponse<List<ProductionDto>>.Ok(data, "Success", pagination);
    }

    private void NormalizeDateFilter(ProductionFilter filter)
    {
        if (!filter.StartDate.HasValue && !filter.EndDate.HasValue)
            return;

        if (filter.StartDate.HasValue && filter.EndDate.HasValue)
        {
            var min = filter.StartDate.Value <= filter.EndDate.Value
                ? filter.StartDate.Value
                : filter.EndDate.Value;

            var max = filter.StartDate.Value >= filter.EndDate.Value
                ? filter.StartDate.Value
                : filter.EndDate.Value;

            filter.StartDate = DateTime.SpecifyKind(min.Date, DateTimeKind.Local).ToUniversalTime();
            filter.EndDate = DateTime.SpecifyKind(max.Date.AddDays(1), DateTimeKind.Local).ToUniversalTime();
        }
        else
        {
            var singleDate = (filter.StartDate ?? filter.EndDate)!.Value.Date;

            filter.StartDate = DateTime.SpecifyKind(singleDate, DateTimeKind.Local).ToUniversalTime();
            filter.EndDate = DateTime.SpecifyKind(singleDate.AddDays(1), DateTimeKind.Local).ToUniversalTime();
        }
    }
}
