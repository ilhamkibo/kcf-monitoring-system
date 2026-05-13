using System;
using KcfMonitoringSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KcfMonitoringSystem.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Machine> Machines => Set<Machine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User → Group (Many-to-One)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Group)
            .WithMany(g => g.Users)
            .HasForeignKey(u => u.GroupId)
            .OnDelete(DeleteBehavior.SetNull);

        // User → Machine (Many-to-One)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Machine)
            .WithMany(m => m.Users)
            .HasForeignKey(u => u.MachineId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
