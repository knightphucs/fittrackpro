namespace FitTrackPro.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Common;
using FitTrackPro.Infrastructure.Persistence.Converters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MediatR;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    private readonly IPublisher _publisher;
    
    public DbSet<UserGoal> UserGoals => Set<UserGoal>();
    public DbSet<Food> Foods => Set<Food>();
    public DbSet<ProgressEntry> ProgressEntries => Set<ProgressEntry>();
    public DbSet<ProgressPhoto> ProgressPhotos => Set<ProgressPhoto>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IPublisher publisher)
        : base(options)
    {
        _publisher = publisher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        // Enforce UTC for all DateTimes
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(property.ClrType == typeof(DateTime) 
                        ? new UtcDateTimeConverter() 
                        : new NullableUtcDateTimeConverter());
                }
            }
        }
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

        var events = ChangeTracker.Entries<IHasDomainEvents>() 
            .Select(x => x.Entity)
            .Where(x => x.DomainEvents.Any())
            .SelectMany(x =>
            {
                var domainEvents = x.DomainEvents.ToList();
                x.ClearDomainEvents();
                return domainEvents;
            })
            .ToList();

            var result = await base.SaveChangesAsync(cancellationToken);

            foreach (var domainEvent in events)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }

        return result;
    }
}
