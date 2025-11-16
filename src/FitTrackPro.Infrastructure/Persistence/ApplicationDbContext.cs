namespace FitTrackPro.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Common;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserGoal> UserGoals => Set<UserGoal>();
    public DbSet<Food> Foods => Set<Food>();
    public DbSet<MealLog> MealLogs => Set<MealLog>();
    public DbSet<ProgressEntry> ProgressEntries => Set<ProgressEntry>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update audit fields
        var entries = ChangeTracker.Entries<IAuditableEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}