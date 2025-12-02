using FitTrackPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitTrackPro.Infrastructure.Persistence.Configurations;

public class ProgressPhotoConfiguration : IEntityTypeConfiguration<ProgressPhoto>
{
    public void Configure(EntityTypeBuilder<ProgressPhoto> builder)
    {
        builder.ToTable("ProgressPhotos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PhotoUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.PhotoType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.Weight)
            .HasPrecision(5, 2);

        builder.HasIndex(p => new { p.UserId, p.TakenAt });

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Ignore(p => p.DomainEvents);
    }
}