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

    public async Task<(List<User> Operators, List<Status> Statuses)> GetDandoriDataAsync(int year)
    {
        var startDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var endDate = new DateTime(year + 1, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        var statuses = await _db.Statuses
            .Include(s => s.Production)
                .ThenInclude(p => p.User)
            .Where(s => s.Code == 3 && s.CreatedAt >= startDate && s.CreatedAt < endDate && (s.Production.User == null || s.Production.User.Name.ToLower() != "testing"))
            .ToListAsync();

        var operators = await _db.Users
            .Where(u => !u.IsDeleted && u.Role == "Operator" && u.Name.ToLower() != "testing")
            .OrderBy(u => u.Id)
            .ToListAsync();

        var statusUserIds = statuses.Select(s => s.Production.UserId).Distinct().ToHashSet();
        var existingUserIds = operators.Select(u => u.Id).ToHashSet();
        var missingUserIds = statusUserIds.Except(existingUserIds).ToList();

        if (missingUserIds.Count > 0)
        {
            var extraUsers = await _db.Users
                .Where(u => missingUserIds.Contains(u.Id) && u.Name.ToLower() != "testing")
                .ToListAsync();
            operators.AddRange(extraUsers);
            operators = operators.OrderBy(u => u.Id).ToList();
        }

        return (operators, statuses);
    }

    public async Task<List<Production>> GetProductionRecordsDataAsync(int year, int? machineId = null, int? productId = null)
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

        if (productId.HasValue)
        {
            query = query.Where(p => p.ProductId == productId.Value);
        }

        return await query
            .OrderBy(p => p.MachineId)
            .ThenBy(p => p.CreatedAt)
            .ToListAsync();
    }
}
