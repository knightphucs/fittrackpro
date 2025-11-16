namespace FitTrackPro.Domain.Entities;

using FitTrackPro.Domain.Common;
using FitTrackPro.Domain.ValueObjects;

public class Food : BaseEntity, IAuditableEntity
{
    public string Name { get; private set; } = default!;
    public string? NameVi { get; private set; } // Vietnamese name
    public string? Category { get; private set; }
    public decimal ServingSize { get; private set; }
    public string ServingUnit { get; private set; } = default!; // bowl, plate, cup, gram
    public int Calories { get; private set; }
    public MacroNutrients Macros { get; private set; } = default!;
    public decimal? Fiber { get; private set; }
    public decimal? Sugar { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsUserCreated { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    private Food() { } // EF Core

    public static Food Create(
        string name,
        string? nameVi,
        string? category,
        decimal servingSize,
        string servingUnit,
        int calories,
        MacroNutrients macros,
        decimal? fiber = null,
        decimal? sugar = null,
        bool isUserCreated = false,
        Guid? createdByUserId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Food name is required", nameof(name));
        if (servingSize <= 0)
            throw new ArgumentException("Serving size must be positive", nameof(servingSize));
        if (calories < 0)
            throw new ArgumentException("Calories cannot be negative", nameof(calories));

        return new Food
        {
            Id = Guid.NewGuid(),
            Name = name,
            NameVi = nameVi,
            Category = category,
            ServingSize = servingSize,
            ServingUnit = servingUnit,
            Calories = calories,
            Macros = macros,
            Fiber = fiber,
            Sugar = sugar,
            IsUserCreated = isUserCreated,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        string? nameVi,
        decimal servingSize,
        int calories,
        MacroNutrients macros)
    {
        Name = name;
        NameVi = nameVi;
        ServingSize = servingSize;
        Calories = calories;
        Macros = macros;
        UpdatedAt = DateTime.UtcNow;
    }
}
