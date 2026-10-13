using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Domain.Entities;
using KcfMonitoringSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KcfMonitoringSystem.Infrastructure.Persistence.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _db;

    public ReportRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<User>> GetOperatorsAsync()
    {
        return await _db.Users
            .Where(u => !u.IsDeleted && u.Role == "Operator" && u.Name.ToLower() != "testing")
            .OrderBy(u => u.Id)
            .ToListAsync();
    }

    public async Task<List<Production>> GetProductionRecordsDataAsync(int year, int? machineId = null)
    {
        var startDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var endDate = new DateTime(year + 1, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        var query = _db.Productions
            .Include(p => p.Machine)
            .Include(p => p.Product)
            .Include(p => p.User)
            .Include(p => p.Statuses)
            .Where(p => p.CreatedAt >= startDate && p.CreatedAt < endDate && (p.User == null || p.User.Name.ToLower() != "testing"));
        // .Where(p => p.CreatedAt >= startDate && p.CreatedAt < endDate);

        if (machineId.HasValue)
        {
            query = query.Where(p => p.MachineId == machineId.Value);
        }

        return await query
            .OrderBy(p => p.MachineId)
            .ThenBy(p => p.Id)
            .ToListAsync();
    }
}
