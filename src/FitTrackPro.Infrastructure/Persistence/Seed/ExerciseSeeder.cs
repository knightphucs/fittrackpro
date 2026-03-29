namespace FitTrackPro.Infrastructure.Persistence.Seed;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Enums;

public class ExerciseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExerciseSeeder> _logger;

    public ExerciseSeeder(ApplicationDbContext context, ILogger<ExerciseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting exercise database seeding...");

            var exercises = GetExercises();
            int inserted = 0;

            foreach (var exercise in exercises)
            {
                bool exists = await _context.Exercises
                    .AnyAsync(e => e.Name == exercise.Name);

                if (!exists)
                {
                    await _context.Exercises.AddAsync(exercise);
                    inserted++;
                }
            }

            if (inserted > 0)
                await _context.SaveChangesAsync();

            _logger.LogInformation("Inserted {Count} new exercises", inserted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding exercises");
            throw;
        }
    }

    private static List<Exercise> GetExercises()
    {
        return new List<Exercise>
        {
            // ========================================
            // CHEST EXERCISES
            // ========================================
            Exercise.Create(
                "Barbell Bench Press",
                "Đẩy tạ đòn nằm",
                ExerciseCategory.Strength,
                MuscleGroup.Chest,
                EquipmentType.Barbell,
                DifficultyLevel.Intermediate,
                "Classic compound chest exercise",
                new List<MuscleGroup> { MuscleGroup.Shoulders, MuscleGroup.Triceps },
                instructions: "Lie on bench, grip bar slightly wider than shoulders, lower to chest, press up"
            ),

            Exercise.Create(
                "Dumbbell Bench Press",
                "Đẩy tạ đơn nằm",
                ExerciseCategory.Strength,
                MuscleGroup.Chest,
                EquipmentType.Dumbbell,
                DifficultyLevel.Intermediate,
                "Dumbbell variation of bench press",
                new List<MuscleGroup> { MuscleGroup.Shoulders, MuscleGroup.Triceps }
            ),

            Exercise.Create(
                "Incline Bench Press",
                "Đẩy tạ nghiêng trên",
                ExerciseCategory.Strength,
                MuscleGroup.Chest,
                EquipmentType.Barbell,
                DifficultyLevel.Intermediate,
                "Targets upper chest",
                new List<MuscleGroup> { MuscleGroup.Shoulders, MuscleGroup.Triceps }
            ),

            Exercise.Create(
                "Push-ups",
                "Chống đẩy",
                ExerciseCategory.Calisthenics,
                MuscleGroup.Chest,
                EquipmentType.Bodyweight,
                DifficultyLevel.Beginner,
                "Classic bodyweight chest exercise",
                new List<MuscleGroup> { MuscleGroup.Shoulders, MuscleGroup.Triceps, MuscleGroup.Abs }
            ),

            Exercise.Create(
                "Cable Chest Fly",
                "Kéo cáp ngực",
                ExerciseCategory.Strength,
                MuscleGroup.Chest,
                EquipmentType.Cable,
                DifficultyLevel.Intermediate,
                "Isolation exercise for chest",
                new List<MuscleGroup> { MuscleGroup.Shoulders }
            ),

            // ========================================
            // BACK EXERCISES
            // ========================================
            Exercise.Create(
                "Deadlift",
                "Nâng tạ đòn",
                ExerciseCategory.Powerlifting,
                MuscleGroup.Back,
                EquipmentType.Barbell,
                DifficultyLevel.Advanced,
                "King of all exercises - full body compound",
                new List<MuscleGroup> { MuscleGroup.Glutes, MuscleGroup.Hamstrings, MuscleGroup.LowerBack, MuscleGroup.Traps }
            ),

            Exercise.Create(
                "Pull-ups",
                "Kéo xà đơn",
                ExerciseCategory.Calisthenics,
                MuscleGroup.Lats,
                EquipmentType.Bodyweight,
                DifficultyLevel.Intermediate,
                "Classic back exercise",
                new List<MuscleGroup> { MuscleGroup.Biceps, MuscleGroup.Shoulders }
            ),

            Exercise.Create(
                "Barbell Row",
                "Chèo tạ đòn",
                ExerciseCategory.Strength,
                MuscleGroup.Back,
                EquipmentType.Barbell,
                DifficultyLevel.Intermediate,
                "Compound back exercise",
                new List<MuscleGroup> { MuscleGroup.Biceps, MuscleGroup.Lats }
            ),

            Exercise.Create(
                "Lat Pulldown",
                "Kéo xô cao",
                ExerciseCategory.Strength,
                MuscleGroup.Lats,
                EquipmentType.Cable,
                DifficultyLevel.Beginner,
                "Machine-based lat exercise",
                new List<MuscleGroup> { MuscleGroup.Biceps }
            ),

            Exercise.Create(
                "Dumbbell Row",
                "Chèo tạ đơn",
                ExerciseCategory.Strength,
                MuscleGroup.Back,
                EquipmentType.Dumbbell,
                DifficultyLevel.Beginner,
                "Unilateral back exercise",
                new List<MuscleGroup> { MuscleGroup.Biceps, MuscleGroup.Lats }
            ),

            // ========================================
            // LEG EXERCISES
            // ========================================
            Exercise.Create(
                "Barbell Squat",
                "Squat tạ đòn",
                ExerciseCategory.Powerlifting,
                MuscleGroup.Quads,
                EquipmentType.Barbell,
                DifficultyLevel.Intermediate,
                "King of leg exercises",
                new List<MuscleGroup> { MuscleGroup.Glutes, MuscleGroup.Hamstrings, MuscleGroup.Abs }
            ),

            Exercise.Create(
                "Romanian Deadlift",
                "Nâng tạ đòn kiểu Romania",
                ExerciseCategory.Strength,
                MuscleGroup.Hamstrings,
                EquipmentType.Barbell,
                DifficultyLevel.Intermediate,
                "Hamstring focused deadlift variation",
                new List<MuscleGroup> { MuscleGroup.Glutes, MuscleGroup.LowerBack }
            ),

            Exercise.Create(
                "Leg Press",
                "Đạp chân máy",
                ExerciseCategory.Strength,
                MuscleGroup.Quads,
                EquipmentType.Machine,
                DifficultyLevel.Beginner,
                "Machine-based leg exercise",
                new List<MuscleGroup> { MuscleGroup.Glutes, MuscleGroup.Hamstrings }
            ),

            Exercise.Create(
                "Lunges",
                "Chùng chân",
                ExerciseCategory.Strength,
                MuscleGroup.Quads,
                EquipmentType.Dumbbell,
                DifficultyLevel.Beginner,
                "Unilateral leg exercise",
                new List<MuscleGroup> { MuscleGroup.Glutes, MuscleGroup.Hamstrings }
            ),

            Exercise.Create(
                "Leg Curl",
                "Gập chân máy",
                ExerciseCategory.Strength,
                MuscleGroup.Hamstrings,
                EquipmentType.Machine,
                DifficultyLevel.Beginner,
                "Isolation exercise for hamstrings"
            ),

            Exercise.Create(
                "Calf Raise",
                "Nhón bắp chân",
                ExerciseCategory.Strength,
                MuscleGroup.Calves,
                EquipmentType.Machine,
                DifficultyLevel.Beginner,
                "Calf isolation exercise"
            ),

            // ========================================
            // SHOULDER EXERCISES
            // ========================================
            Exercise.Create(
                "Overhead Press",
                "Đẩy tạ qua đầu",
                ExerciseCategory.Strength,
                MuscleGroup.Shoulders,
                EquipmentType.Barbell,
                DifficultyLevel.Intermediate,
                "Compound shoulder exercise",
                new List<MuscleGroup> { MuscleGroup.Triceps, MuscleGroup.Traps }
            ),

            Exercise.Create(
                "Lateral Raise",
                "Nâng tạ sang ngang",
                ExerciseCategory.Strength,
                MuscleGroup.Shoulders,
                EquipmentType.Dumbbell,
                DifficultyLevel.Beginner,
                "Isolation exercise for side delts"
            ),

            Exercise.Create(
                "Front Raise",
                "Nâng tạ phía trước",
                ExerciseCategory.Strength,
                MuscleGroup.Shoulders,
                EquipmentType.Dumbbell,
                DifficultyLevel.Beginner,
                "Isolation exercise for front delts"
            ),

            // ========================================
            // ARM EXERCISES
            // ========================================
            Exercise.Create(
                "Barbell Curl",
                "Cuốn tạ đòn",
                ExerciseCategory.Strength,
                MuscleGroup.Biceps,
                EquipmentType.Barbell,
                DifficultyLevel.Beginner,
                "Classic bicep exercise"
            ),

            Exercise.Create(
                "Dumbbell Curl",
                "Cuốn tạ đơn",
                ExerciseCategory.Strength,
                MuscleGroup.Biceps,
                EquipmentType.Dumbbell,
                DifficultyLevel.Beginner,
                "Basic bicep curl"
            ),

            Exercise.Create(
                "Hammer Curl",
                "Cuốn búa",
                ExerciseCategory.Strength,
                MuscleGroup.Biceps,
                EquipmentType.Dumbbell,
                DifficultyLevel.Beginner,
                "Neutral grip bicep curl",
                new List<MuscleGroup> { MuscleGroup.Forearms }
            ),

            Exercise.Create(
                "Tricep Dips",
                "Chống song song",
                ExerciseCategory.Calisthenics,
                MuscleGroup.Triceps,
                EquipmentType.Bodyweight,
                DifficultyLevel.Intermediate,
                "Bodyweight tricep exercise",
                new List<MuscleGroup> { MuscleGroup.Chest, MuscleGroup.Shoulders }
            ),

            Exercise.Create(
                "Tricep Pushdown",
                "Đẩy cáp tay sau",
                ExerciseCategory.Strength,
                MuscleGroup.Triceps,
                EquipmentType.Cable,
                DifficultyLevel.Beginner,
                "Cable tricep isolation"
            ),

            Exercise.Create(
                "Skull Crushers",
                "Đập sọ",
                ExerciseCategory.Strength,
                MuscleGroup.Triceps,
                EquipmentType.Barbell,
                DifficultyLevel.Intermediate,
                "Lying tricep extension"
            ),

            // ========================================
            // CORE EXERCISES
            // ========================================
            Exercise.Create(
                "Plank",
                "Chống tay chân",
                ExerciseCategory.Calisthenics,
                MuscleGroup.Abs,
                EquipmentType.Bodyweight,
                DifficultyLevel.Beginner,
                "Isometric core exercise",
                new List<MuscleGroup> { MuscleGroup.Shoulders }
            ),

            Exercise.Create(
                "Crunches",
                "Gập bụng",
                ExerciseCategory.Calisthenics,
                MuscleGroup.Abs,
                EquipmentType.Bodyweight,
                DifficultyLevel.Beginner,
                "Basic ab exercise"
            ),

            Exercise.Create(
                "Russian Twists",
                "Xoay người Nga",
                ExerciseCategory.Calisthenics,
                MuscleGroup.Obliques,
                EquipmentType.Bodyweight,
                DifficultyLevel.Beginner,
                "Oblique exercise",
                new List<MuscleGroup> { MuscleGroup.Abs }
            ),

            Exercise.Create(
                "Hanging Leg Raises",
                "Nâng chân treo xà",
                ExerciseCategory.Calisthenics,
                MuscleGroup.Abs,
                EquipmentType.Bodyweight,
                DifficultyLevel.Advanced,
                "Advanced ab exercise"
            ),

            // ========================================
            // CARDIO EXERCISES
            // ========================================
            Exercise.Create(
                "Running",
                "Chạy bộ",
                ExerciseCategory.Cardio,
                MuscleGroup.Cardio,
                EquipmentType.None,
                DifficultyLevel.Beginner,
                "Cardiovascular exercise"
            ),

            Exercise.Create(
                "Cycling",
                "Đạp xe",
                ExerciseCategory.Cardio,
                MuscleGroup.Cardio,
                EquipmentType.Bike,
                DifficultyLevel.Beginner,
                "Low impact cardio",
                new List<MuscleGroup> { MuscleGroup.Quads, MuscleGroup.Hamstrings }
            ),

            Exercise.Create(
                "Rowing",
                "Chèo thuyền máy",
                ExerciseCategory.Cardio,
                MuscleGroup.FullBody,
                EquipmentType.Rower,
                DifficultyLevel.Intermediate,
                "Full body cardio",
                new List<MuscleGroup> { MuscleGroup.Back, MuscleGroup.Legs }
            ),

            Exercise.Create(
                "Jump Rope",
                "Nhảy dây",
                ExerciseCategory.Cardio,
                MuscleGroup.Cardio,
                EquipmentType.None,
                DifficultyLevel.Beginner,
                "High intensity cardio",
                new List<MuscleGroup> { MuscleGroup.Calves }
            ),

            Exercise.Create(
                "Burpees",
                "Burpee",
                ExerciseCategory.Plyometric,
                MuscleGroup.FullBody,
                EquipmentType.Bodyweight,
                DifficultyLevel.Intermediate,
                "Full body conditioning exercise"
            )
        };
    }
}