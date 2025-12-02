namespace FitTrackPro.Application.Common.Interfaces;

using Microsoft.EntityFrameworkCore;
using FitTrackPro.Domain.Entities;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<UserGoal> UserGoals { get; }
    DbSet<Food> Foods { get; }
    DbSet<MealLog> MealLogs { get; }
    DbSet<ProgressEntry> ProgressEntries { get; }
    DbSet<ProgressPhoto> ProgressPhotos { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
