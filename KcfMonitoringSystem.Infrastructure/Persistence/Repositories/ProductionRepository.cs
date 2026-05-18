using System;
using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KcfMonitoringSystem.Infrastructure.Persistence.Repositories;

public class ProductionRepository : IProductionRepository
{
    private readonly AppDbContext _db;

    public ProductionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(List<Production> Data, int TotalCount)> GetAllAsync(ProductionFilter filter)
    {
        var query = _db.Productions
            .Include(p => p.Machine)
            .Include(p => p.User)
            .Include(p => p.Product)
            .AsQueryable();

        if (filter.MachineId.HasValue)
        {
            query = query.Where(x => x.MachineId == filter.MachineId.Value);
        }

        if (filter.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == filter.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(x => x.Machine.Name.ToLower().Contains(search) ||
                                     x.User.Name.ToLower().Contains(search) ||
                                     (x.Product != null && x.Product.ProductNo.ToLower().Contains(search)) ||
                                     (x.Product != null && x.Product.PartName.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync();

        query = query.OrderByDescending(x => x.CreatedAt);

        if (filter.Paginate == true)
        {
            query = query.Skip((filter.Page - 1) * filter.Limit).Take(filter.Limit);
        }

        var data = await query.ToListAsync();

        return (data, totalCount);
    }

    public async Task<Production?> GetByIdAsync(int id)
    {
        return await _db.Productions
            .Include(p => p.Machine)
            .Include(p => p.User)
            .Include(p => p.Product)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
