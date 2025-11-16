namespace FitTrackPro.Domain.Entities;

using FitTrackPro.Domain.Common;

public class ProgressEntry : BaseEntity, IAuditableEntity
{
    public Guid UserId { get; private set; }
    public decimal Weight { get; private set; } // kg
    public decimal? BodyFatPercentage { get; private set; }
    public decimal? Chest { get; private set; } // cm
    public decimal? Waist { get; private set; }
    public decimal? Hips { get; private set; }
    public decimal? Arms { get; private set; }
    public decimal? Legs { get; private set; }
    public string? Notes { get; private set; }
    public DateTime RecordedAt { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation
    public User User { get; private set; } = default!;

    private ProgressEntry() { } // EF Core

    public static ProgressEntry Create(
        Guid userId,
        decimal weight,
        DateTime recordedAt,
        decimal? bodyFatPercentage = null,
        decimal? chest = null,
        decimal? waist = null,
        decimal? hips = null,
        decimal? arms = null,
        decimal? legs = null,
        string? notes = null)
    {
        if (weight <= 0)
            throw new ArgumentException("Weight must be positive", nameof(weight));

        return new ProgressEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Weight = weight,
            BodyFatPercentage = bodyFatPercentage,
            Chest = chest,
            Waist = waist,
            Hips = hips,
            Arms = arms,
            Legs = legs,
            Notes = notes,
            RecordedAt = recordedAt,
            CreatedAt = DateTime.UtcNow
        };
    }
}