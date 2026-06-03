using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KcfMonitoringSystem.Infrastructure.Persistence.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly AppDbContext _db;

    public GroupRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(List<Group> Data, int TotalCount)> GetAllAsync(GroupFilter filter)
    {
        var query = _db.Groups.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        query = query.OrderBy(x => x.Id);

        if (filter.Paginate == true)
        {
            query = query.Skip((filter.Page - 1) * filter.Limit).Take(filter.Limit);
        }

        var data = await query.ToListAsync();

        return (data, totalCount);
    }

    public async Task<Group?> GetByIdAsync(int id)
    {
        return await _db.Groups.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task CreateAsync(Group group)
    {
        await _db.Groups.AddAsync(group);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, Group group)
    {
        var existingGroup = await _db.Groups.FirstOrDefaultAsync(x => x.Id == id);
        if (existingGroup == null)
            return;

        existingGroup.Name = group.Name.Trim();
        existingGroup.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Group group)
    {
        _db.Groups.Remove(group);
        await _db.SaveChangesAsync();
    }
}
