namespace FitTrackPro.Domain.ValueObjects;

public record MacroNutrients
{
    public decimal Protein { get; init; }  // grams
    public decimal Carbs { get; init; }    // grams
    public decimal Fat { get; init; }      // grams

    public MacroNutrients(decimal protein, decimal carbs, decimal fat)
    {
        if (protein < 0) throw new ArgumentException("Protein cannot be negative", nameof(protein));
        if (carbs < 0) throw new ArgumentException("Carbs cannot be negative", nameof(carbs));
        if (fat < 0) throw new ArgumentException("Fat cannot be negative", nameof(fat));

        Protein = protein;
        Carbs = carbs;
        Fat = fat;
    }

    public int CalculateCalories()
    {
        // Protein: 4 cal/g, Carbs: 4 cal/g, Fat: 9 cal/g
        return (int)Math.Round((Protein * 4) + (Carbs * 4) + (Fat * 9));
    }

    public static MacroNutrients Zero => new(0, 0, 0);
}