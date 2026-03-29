namespace FitTrackPro.Application.Common.Interfaces;

using Microsoft.EntityFrameworkCore;
using FitTrackPro.Domain.Entities;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<UserGoal> UserGoals { get; }
    DbSet<Food> Foods { get; }
    DbSet<ProgressEntry> ProgressEntries { get; }
    DbSet<ProgressPhoto> ProgressPhotos { get; }
    DbSet<Exercise> Exercises { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
