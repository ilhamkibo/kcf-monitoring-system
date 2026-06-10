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
    public DbSet<Production> Productions => Set<Production>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Status> Statuses => Set<Status>();
    public DbSet<AlarmHistory> AlarmHistories => Set<AlarmHistory>();

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

        // Production → Machine (Many-to-One)
        modelBuilder.Entity<Production>()
            .HasOne(p => p.Machine)
            .WithMany(m => m.Productions)
            .HasForeignKey(p => p.MachineId)
            .OnDelete(DeleteBehavior.Cascade);

        // Production → User (Many-to-One)
        modelBuilder.Entity<Production>()
            .HasOne(p => p.User)
            .WithMany(u => u.Productions)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Production → Product (Many-to-One)
        modelBuilder.Entity<Production>()
            .HasOne(p => p.Product)
            .WithMany(p => p.Productions)
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        // Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.ProductNo).IsUnique();
            entity.Property(p => p.ProductNo).IsRequired().HasMaxLength(50);
            entity.Property(p => p.PartName).IsRequired().HasMaxLength(200);
            entity.Property(p => p.PartNo).IsRequired().HasMaxLength(100);
        });

        // Status -> Machine (Many-to-One)
        modelBuilder.Entity<Status>()
            .HasOne(s => s.Machine)
            .WithMany(m => m.Statuses)
            .HasForeignKey(s => s.MachineId)
            .OnDelete(DeleteBehavior.Cascade);

        // Status → User (Many-to-One)
        modelBuilder.Entity<Status>()
            .HasOne(p => p.User)
            .WithMany(u => u.Statuses)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Status → Product (Many-to-One)
        modelBuilder.Entity<Status>()
            .HasOne(p => p.Product)
            .WithMany(p => p.Statuses)
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // AlarmHistory → Machine (Many-to-One)
        modelBuilder.Entity<AlarmHistory>()
            .HasOne(a => a.Machine)
            .WithMany(m => m.AlarmHistories)
            .HasForeignKey(a => a.MachineId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
