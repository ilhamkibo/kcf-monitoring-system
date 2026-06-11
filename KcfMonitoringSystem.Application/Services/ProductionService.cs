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

    public async Task<ApiPagedResponse<List<ProductionDto>>> GetAllAsync(ProductionFilter filter)
    {
        var (start, end) = DateFilterHelper.Normalize(filter.StartDate, filter.EndDate);
        filter.StartDate = start;
        filter.EndDate = end;

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
            x.CreatedAt
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

        return ApiPagedResponse<List<ProductionDto>>.Ok(data, "Success", pagination);
    }
}
