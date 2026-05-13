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
        var query = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(search) || 
                                     x.Email.ToLower().Contains(search) ||
                                     x.Username.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        if (filter.Paginate == true)
        {
            query = query.Skip((filter.Page - 1) * filter.Limit).Take(filter.Limit);
        }

        var data = await query.OrderBy(x => x.Id).ToListAsync();

        return (data, totalCount);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _db.Users
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}