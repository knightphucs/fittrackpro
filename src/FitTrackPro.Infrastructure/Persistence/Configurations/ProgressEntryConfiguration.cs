namespace FitTrackPro.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitTrackPro.Domain.Entities;

public class ProgressEntryConfiguration : IEntityTypeConfiguration<ProgressEntry>
{
    public void Configure(EntityTypeBuilder<ProgressEntry> builder)
    {
        builder.ToTable("ProgressEntries");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Weight)
            .HasPrecision(5, 2);

        builder.Property(p => p.BodyFatPercentage)
            .HasPrecision(5, 2);

        builder.Property(p => p.Chest)
            .HasPrecision(5, 2);

        builder.Property(p => p.Waist)
            .HasPrecision(5, 2);

        builder.Property(p => p.Hips)
            .HasPrecision(5, 2);

        builder.Property(p => p.Arms)
            .HasPrecision(5, 2);

        builder.Property(p => p.Legs)
            .HasPrecision(5, 2);

        builder.HasIndex(p => new { p.UserId, p.RecordedAt });

        builder.HasOne(p => p.User)
            .WithMany(u => u.ProgressEntries)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(p => p.DomainEvents);
    }
}
