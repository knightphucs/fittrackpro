using FitTrackPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitTrackPro.Infrastructure.Persistence.Configurations;

public class UserGoalConfiguration : IEntityTypeConfiguration<UserGoal>
{
    public void Configure(EntityTypeBuilder<UserGoal> builder)
    {
        builder.ToTable("UserGoals");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.CurrentWeight)
            .HasPrecision(5, 2);

        builder.Property(g => g.TargetWeight)
            .HasPrecision(5, 2);

        builder.Property(g => g.ActivityLevel)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(g => g.WeightGoal)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.OwnsOne(g => g.TargetMacros, macros =>
        {
            macros.Property(m => m.Protein)
                .HasColumnName("TargetProtein")
                .HasPrecision(5, 2);

            macros.Property(m => m.Carbs)
                .HasColumnName("TargetCarbs")
                .HasPrecision(5, 2);

            macros.Property(m => m.Fat)
                .HasColumnName("TargetFat")
                .HasPrecision(5, 2);
        });

        builder.HasOne(g => g.User)
            .WithOne(u => u.CurrentGoal)
            .HasForeignKey<UserGoal>(g => g.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(g => new { g.UserId, g.IsActive });

        builder.Ignore(g => g.DomainEvents);
    }
}
