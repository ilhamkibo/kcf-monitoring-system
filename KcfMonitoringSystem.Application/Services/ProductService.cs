using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Repositories;

namespace KcfMonitoringSystem.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<List<ProductDto>>> GetAllAsync(ProductFilter filter)
    {
        var (products, totalCount) = await _repository.GetAllAsync(filter);

        var data = products.Select(x => new ProductDto(
            x.Id,
            x.ProductNo,
            x.PartName,
            x.PartNo,
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

        return ApiResponse<List<ProductDto>>.Ok(data, "Success", pagination);
    }

    public async Task<ApiResponse<ProductDto>> GetByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);

        if (product == null)
            return ApiResponse<ProductDto>.Error("Product not found");

        var data = new ProductDto(
            product.Id,
            product.ProductNo,
            product.PartName,
            product.PartNo,
            product.CreatedAt.ToLocalTime()
        );

        return ApiResponse<ProductDto>.Ok(data);
    }

    public async Task<ApiResponse<ProductDto>> GetByProductNoAsync(string productNo)
    {
        var product = await _repository.GetByProductNoAsync(productNo);

        if (product == null)
            return ApiResponse<ProductDto>.Error("Product not found");

        var data = new ProductDto(
            product.Id,
            product.ProductNo,
            product.PartName,
            product.PartNo,
            product.CreatedAt.ToLocalTime()
        );

        return ApiResponse<ProductDto>.Ok(data);
    }
}
