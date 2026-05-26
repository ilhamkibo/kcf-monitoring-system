using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KcfMonitoringSystem.Infrastructure.Persistence.Repositories;

public class MachineRepository : IMachineRepository
{
    private readonly AppDbContext _db;

    public MachineRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(List<Machine> Data, int TotalCount)> GetAllAsync(MachineFilter filter)
    {
        var query = _db.Machines.AsQueryable();

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

    public async Task<Machine?> GetByIdAsync(int id)
    {
        return await _db.Machines.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task CreateAsync(Machine machine)
    {
        await _db.Machines.AddAsync(machine);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, Machine machine)
    {
        var existingMachine = await _db.Machines.FirstOrDefaultAsync(x => x.Id == id);
        if (existingMachine == null)
            return;

        existingMachine.Name = machine.Name.Trim();
        existingMachine.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Machine machine)
    {
        _db.Machines.Remove(machine);
        await _db.SaveChangesAsync();
    }
}