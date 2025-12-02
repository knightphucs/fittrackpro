using FitTrackPro.Domain.Common;
using FitTrackPro.Domain.Enums;

namespace FitTrackPro.Domain.Entities;

public class ProgressPhoto : BaseEntity, IAuditableEntity
{
    public Guid UserId { get; private set; }
    public string PhotoUrl { get; private set; } = default!;
    public string PhotoType { get; private set; } = default!; // e.g., "front", "side", "back"
    public DateTime TakenAt { get; private set; }
    public decimal? Weight { get; private set; } // Optional weight at the time of the photo
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation property
    public User User { get; private set; } = default!;

    public ProgressPhoto() {}

    public static ProgressPhoto Create(
        Guid userId,
        string photoUrl,
        string photoType,
        DateTime takenAt,
        decimal? weight = null,
        string? notes = null
    )
    {
        if (string.IsNullOrWhiteSpace(photoUrl))
            throw new ArgumentException("Photo URL is required.", nameof(photoUrl));

        var validTypes = new[] { "Front", "Side", "Back" };
        if (!validTypes.Contains(photoType, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"Photo type must be one of the following: {string.Join(", ", validTypes)}.", nameof(photoType));

        return new ProgressPhoto
        {
            UserId = userId,
            PhotoUrl = photoUrl,
            PhotoType = photoType,
            TakenAt = takenAt,
            Weight = weight,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdatePhotoUrl(string newPhotoUrl)
    {
        PhotoUrl = newPhotoUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateWeight(decimal weight)
    {
        Weight = weight;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeType(string newType)
    {
        PhotoType = newType;
        UpdatedAt = DateTime.UtcNow;
    }
}