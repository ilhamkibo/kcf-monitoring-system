using System;
using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KcfMonitoringSystem.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(List<User> Data, int TotalCount)> GetAllAsync(UserFilter filter)
    {
        var query = _db.Users
            .Include(u => u.Group)
            .Include(u => u.Machine)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(search) ||
                                     (x.Email != null && x.Email.ToLower().Contains(search)) ||
                                     (x.Username != null && x.Username.ToLower().Contains(search)) ||
                                     (x.Role != null && x.Role.ToLower().Contains(search)));
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

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _db.Users
            .Include(u => u.Group)
            .Include(u => u.Machine)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(User user)
    {
        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> GroupExistsAsync(int groupId)
    {
        return await _db.Groups.AnyAsync(g => g.Id == groupId);
    }

    public async Task<bool> MachineExistsAsync(int machineId)
    {
        return await _db.Machines.AnyAsync(m => m.Id == machineId);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _db.Users.AnyAsync(u => u.Username != null && u.Username.ToLower() == username.ToLower());
    }
}