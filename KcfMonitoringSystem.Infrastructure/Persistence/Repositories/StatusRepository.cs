using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Domain.Entities;
using KcfMonitoringSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KcfMonitoringSystem.Infrastructure.Persistence.Repositories;

public class StatusRepository : IStatusRepository
{
    private readonly AppDbContext _db;

    public StatusRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(List<Status> Data, int TotalCount)> GetAllAsync(StatusFilter filter)
    {
        var query = _db.Statuses
            .Include(x => x.Machine)
            .AsQueryable();

        if (filter.MachineId.HasValue)
            query = query.Where(x => x.MachineId == filter.MachineId.Value);

        if (filter.Code.HasValue)
            query = query.Where(x => x.Code == filter.Code.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(x => x.CreatedAt >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(x => x.CreatedAt < filter.EndDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(x => x.Machine.Name.ToLower().Contains(search));
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
}