using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Domain.Entities;

namespace KcfMonitoringSystem.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<(List<Product> Data, int TotalCount)> GetAllAsync(ProductFilter filter);
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetByProductNoAsync(string productNo);
    Task<Product> AddAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task DeleteAsync(Product product);
}
