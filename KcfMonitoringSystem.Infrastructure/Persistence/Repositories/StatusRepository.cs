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
            .Include(x => x.Production)
                .ThenInclude(p => p.User)
            .Include(x => x.Production)
                .ThenInclude(p => p.Product)
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

    public async Task<List<Status>> GetTimelineStatusesAsync(StatusFilter filter)
    {
        var query = _db.Statuses
            .Include(x => x.Machine)
            .Include(x => x.Production)
                .ThenInclude(p => p.User)
            .Include(x => x.Production)
                .ThenInclude(p => p.Product)
            .AsQueryable();

        if (filter.MachineId.HasValue)
            query = query.Where(x => x.MachineId == filter.MachineId.Value);

        if (filter.Code.HasValue)
            query = query.Where(x => x.Code == filter.Code.Value);

        // Include statuses that overlap with the date range:
        // status [CreatedAt, UpdatedAt] overlaps with [StartDate, EndDate]
        // StartDate filter: check UpdatedAt >= StartDate (if null, use CreatedAt)
        // EndDate filter: check CreatedAt < EndDate
        if (filter.StartDate.HasValue)
            query = query.Where(x => (x.UpdatedAt ?? x.CreatedAt) >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(x => x.CreatedAt < filter.EndDate.Value);

        if (filter.UserId.HasValue)
            query = query.Where(x => x.Production.UserId == filter.UserId.Value);

        if (filter.ProductId.HasValue)
            query = query.Where(x => x.Production.ProductId == filter.ProductId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(x => x.Machine.Name.ToLower().Contains(search));
        }

        return await query
            .OrderBy(x => x.MachineId)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Status>> GetActivityStatusesAsync(StatusFilter filter)
    {
        var query = _db.Statuses
            .Include(x => x.Production)
                .ThenInclude(p => p.User)
            .Include(x => x.Production)
                .ThenInclude(p => p.Product)
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

        if (filter.UserId.HasValue)
            query = query.Where(x => x.Production.UserId == filter.UserId.Value);

        if (filter.ProductId.HasValue)
            query = query.Where(x => x.Production.ProductId == filter.ProductId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(x => x.Machine.Name.ToLower().Contains(search) ||
                                     x.Production.User.Name.ToLower().Contains(search) ||
                                     x.Production.Product!.PartName.ToLower().Contains(search));
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
}