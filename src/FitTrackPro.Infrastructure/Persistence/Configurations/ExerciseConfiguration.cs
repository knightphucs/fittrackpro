namespace FitTrackPro.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Enums;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("Exercises");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.NameVi)
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Category)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.PrimaryMuscle)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.SecondaryMuscles)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<List<MuscleGroup>>(v, (JsonSerializerOptions)null!) ?? new())
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(
                new ValueComparer<List<MuscleGroup>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()
                )
            );

        builder.Property(e => e.Equipment)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Difficulty)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.VideoUrl)
            .HasMaxLength(500);

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(500);

        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.PrimaryMuscle);

        builder.Ignore(e => e.DomainEvents);
    }
}
