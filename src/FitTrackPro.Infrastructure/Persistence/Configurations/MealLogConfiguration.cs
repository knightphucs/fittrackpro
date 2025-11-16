namespace FitTrackPro.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitTrackPro.Domain.Entities;

public class MealLogConfiguration : IEntityTypeConfiguration<MealLog>
{
    public void Configure(EntityTypeBuilder<MealLog> builder)
    {
        builder.ToTable("MealLogs");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.ServingSize)
            .HasPrecision(10, 2);

        builder.Property(m => m.ServingMultiplier)
            .HasPrecision(5, 2);

        builder.HasIndex(m => new { m.UserId, m.LoggedAt });

        builder.HasOne(m => m.User)
            .WithMany(u => u.MealLogs)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Food)
            .WithMany()
            .HasForeignKey(m => m.FoodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(m => m.DomainEvents);
    }
}