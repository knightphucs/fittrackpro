namespace FitTrackPro.Domain.Entities;

using FitTrackPro.Domain.Common;
using FitTrackPro.Domain.Enums;

public class Exercise : BaseEntity, IAuditableEntity
{
    public string Name { get; private set; } = default!;
    public string? NameVi { get; private set; }
    public string? Description { get; private set; }
    public ExerciseCategory Category { get; private set; }
    public MuscleGroup PrimaryMuscle { get; private set; }
    public List<MuscleGroup> SecondaryMuscles { get; private set; } = new();
    public EquipmentType Equipment { get; private set; }
    public DifficultyLevel Difficulty { get; private set; }
    public string? VideoUrl { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? Instructions { get; private set; }
    public bool IsUserCreated { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    private Exercise() { } // EF Core

    public static Exercise Create(
        string name,
        string? nameVi,
        ExerciseCategory category,
        MuscleGroup primaryMuscle,
        EquipmentType equipment,
        DifficultyLevel difficulty,
        string? description = null,
        List<MuscleGroup>? secondaryMuscles = null,
        string? videoUrl = null,
        string? imageUrl = null,
        string? instructions = null,
        bool isUserCreated = false,
        Guid? createdByUserId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Exercise name is required", nameof(name));

        return new Exercise
        {
            Id = Guid.NewGuid(),
            Name = name,
            NameVi = nameVi,
            Category = category,
            PrimaryMuscle = primaryMuscle,
            SecondaryMuscles = secondaryMuscles ?? new List<MuscleGroup>(),
            Equipment = equipment,
            Difficulty = difficulty,
            Description = description,
            VideoUrl = videoUrl,
            ImageUrl = imageUrl,
            Instructions = instructions,
            IsUserCreated = isUserCreated,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        string? nameVi,
        string? description,
        ExerciseCategory category,
        MuscleGroup primaryMuscleGroup,
        EquipmentType equipment,
        DifficultyLevel difficulty,
        List<MuscleGroup>? secondaryMuscleGroups = null)
    {
        Name = name;
        NameVi = nameVi;
        Description = description;
        Category = category;
        PrimaryMuscle = primaryMuscleGroup;
        Equipment = equipment;
        Difficulty = difficulty;
        SecondaryMuscles = secondaryMuscleGroups ?? new();
        UpdatedAt = DateTime.UtcNow;
    }
}
