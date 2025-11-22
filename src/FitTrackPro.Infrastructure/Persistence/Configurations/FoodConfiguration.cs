namespace FitTrackPro.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitTrackPro.Domain.Entities;

public class FoodConfiguration : IEntityTypeConfiguration<Food>
{
    public void Configure(EntityTypeBuilder<Food> builder)
    {
        builder.ToTable("Foods");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(f => f.NameVi)
            .HasMaxLength(255);

        builder.HasIndex(f => f.Name);
        builder.HasIndex(f => f.NameVi);

        builder.Property(f => f.Category)
            .HasMaxLength(100);

        builder.Property(f => f.ServingSize)
            .HasPrecision(10, 2);

        builder.Property(f => f.ServingUnit)
            .IsRequired()
            .HasMaxLength(50);

        builder.OwnsOne(f => f.Macros, macros =>
        {
            macros.Property(m => m.Protein)
                .HasColumnName("Protein")
                .HasPrecision(5, 2);

            macros.Property(m => m.Carbs)
                .HasColumnName("Carbs")
                .HasPrecision(5, 2);

            macros.Property(m => m.Fat)
                .HasColumnName("Fat")
                .HasPrecision(5, 2);
        });

        builder.Property(f => f.Fiber)
            .HasPrecision(5, 2);

        builder.Property(f => f.Sugar)
            .HasPrecision(5, 2);

        builder.Ignore(f => f.DomainEvents);
    }
}
