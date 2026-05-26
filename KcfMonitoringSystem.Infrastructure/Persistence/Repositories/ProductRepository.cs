using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KcfMonitoringSystem.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(List<Product> Data, int TotalCount)> GetAllAsync(ProductFilter filter)
    {
        var query = _db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(x =>
                x.ProductNo.ToLower().Contains(search) ||
                x.PartName.ToLower().Contains(search) ||
                x.PartNo.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        query = query.OrderBy(x => x.ProductNo);

        if (filter.Paginate == true)
        {
            query = query.Skip((filter.Page - 1) * filter.Limit).Take(filter.Limit);
        }

        var data = await query.ToListAsync();

        return (data, totalCount);
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Product?> GetByProductNoAsync(string productNo)
    {
        return await _db.Products.FirstOrDefaultAsync(x => x.ProductNo == productNo);
    }

    public async Task<Product> AddAsync(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;
        _db.Products.Update(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task DeleteAsync(Product product)
    {
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
    }
}
