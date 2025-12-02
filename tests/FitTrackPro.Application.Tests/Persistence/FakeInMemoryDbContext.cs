using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitTrackPro.Application.Tests.Persistence;

public class FakeInMemoryDbContext : DbContext, IApplicationDbContext
{
    public FakeInMemoryDbContext(DbContextOptions<FakeInMemoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserGoal> UserGoals => Set<UserGoal>();
    public DbSet<MealLog> MealLogs => Set<MealLog>();
    public DbSet<ProgressEntry> ProgressEntries => Set<ProgressEntry>();
    public DbSet<Food> Foods => Set<Food>();
    public DbSet<ProgressPhoto> ProgressPhotos => Set<ProgressPhoto>();

    // Override to satisfy interface
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => base.SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tell EF that Food is a proper entity
        modelBuilder.Entity<Food>();

        // owned type for Food
        modelBuilder.Entity<Food>().OwnsOne(f => f.Macros);

        // Tell EF that UserGoal is a proper entity
        modelBuilder.Entity<UserGoal>();

        // owned type for UserGoal
        modelBuilder.Entity<UserGoal>().OwnsOne(u => u.TargetMacros);
    }
}
