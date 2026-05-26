using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;

namespace KcfMonitoringSystem.Application.Services;

public interface IProductService
{
    Task<ApiPagedResponse<List<ProductDto>>> GetAllAsync(ProductFilter filter);
    Task<ApiResponse<ProductDto>> GetByIdAsync(int id);
    Task<ApiResponse<ProductDto>> GetByProductNoAsync(string productNo);
    Task<ApiResponse<ProductDto>> CreateAsync(CreateProductDto dto);
    Task<ApiResponse<ProductDto>> UpdateAsync(int id, UpdateProductDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
