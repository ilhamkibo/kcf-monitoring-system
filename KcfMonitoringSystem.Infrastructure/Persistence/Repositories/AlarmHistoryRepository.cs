using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KcfMonitoringSystem.Infrastructure.Persistence.Repositories;

public class AlarmHistoryRepository : IAlarmHistoryRepository
{
    private readonly AppDbContext _db;

    public AlarmHistoryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(List<AlarmHistory> Data, int TotalCount)> GetAllAsync(AlarmHistoryFilter filter)
    {
        var query = _db.AlarmHistories
            .Include(x => x.Machine)
            .AsQueryable();

        if (filter.MachineId.HasValue)
        {
            query = query.Where(x => x.MachineId == filter.MachineId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = filter.Status.ToLower();
            query = query.Where(x => x.Status.ToLower() == status);
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(x => x.Timestamp >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(x => x.Timestamp < filter.EndDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(x => x.Message.ToLower().Contains(search) || x.Machine.Name.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        query = query.OrderByDescending(x => x.Timestamp);

        if (filter.Paginate == true)
        {
            query = query.Skip((filter.Page - 1) * filter.Limit).Take(filter.Limit);
        }

        var data = await query.ToListAsync();

        return (data, totalCount);
    }
}
