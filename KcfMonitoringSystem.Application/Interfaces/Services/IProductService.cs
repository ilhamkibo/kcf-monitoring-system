using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;

namespace KcfMonitoringSystem.Application.Services;

public interface IProductService
{
    Task<ApiResponse<List<ProductDto>>> GetAllAsync(ProductFilter filter);
    Task<ApiResponse<ProductDto>> GetByIdAsync(int id);
    Task<ApiResponse<ProductDto>> GetByProductNoAsync(string productNo);
}
