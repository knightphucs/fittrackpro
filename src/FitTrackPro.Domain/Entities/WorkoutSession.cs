namespace FitTrackPro.Domain.Entities;

using FitTrackPro.Domain.Common;
using FitTrackPro.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class WorkoutSession
{
    [BsonId]
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = default!;
    public string? Notes { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public int DurationMinutes { get; private set; }
    public int TotalCaloriesBurned { get; private set; }

    [BsonRepresentation(BsonType.String)]
    public WorkoutStatus Status { get; private set; }

    [BsonIgnore]
    public bool IsCompleted => Status == WorkoutStatus.Completed;
    
    // Navigation
    public List<WorkoutExercise> Exercises { get; private set; } = new List<WorkoutExercise>();
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public WorkoutSession() { }

    public static WorkoutSession Create(
        Guid userId,
        string title,
        DateTime startedAt,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Workout title is required", nameof(title));

        return new WorkoutSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Notes = notes,
            StartedAt = startedAt,
            Status = WorkoutStatus.InProgress,
            CreatedAt = DateTime.UtcNow,
            Exercises = new List<WorkoutExercise>()
        };
    }

    public WorkoutExercise AddOrUpdateExercise(WorkoutExercise exercise)
    {
        var existingExercise = Exercises
            .FirstOrDefault(e => e.ExerciseId == exercise.ExerciseId);

        if (existingExercise != null)
        {
            foreach (var set in exercise.Sets)
            {
                var nextSetNumber = existingExercise.Sets.Any() 
                    ? existingExercise.Sets.Max(s => s.SetNumber) + 1 
                    : 1;
            
                var newSet = ExerciseSet.Create(
                    nextSetNumber, 
                    set.Weight, 
                    set.Reps, 
                    set.DurationSeconds, 
                    set.Distance
                );
                
                existingExercise.AddSet(newSet);
            }
            
            if (!string.IsNullOrEmpty(exercise.Notes)) 
            {
                existingExercise.UpdateNotes(exercise.Notes); 
            }

            UpdatedAt = DateTime.UtcNow;
            return existingExercise;
        }
        else
        {
            var nextOrderIdx = Exercises.Any() ? Exercises.Max(e => e.OrderIndex) + 1 : 1;
            exercise.SetOrderIndex(nextOrderIdx);
            Exercises.Add(exercise);
            UpdatedAt = DateTime.UtcNow;
            return exercise;
        }
    }

    public void AddExercise(WorkoutExercise exercise) => AddOrUpdateExercise(exercise);

    public void Complete(DateTime endedAt, int caloriesBurned)
    {
        EndedAt = endedAt;
        DurationMinutes = (int)(endedAt - StartedAt).TotalMinutes;
        TotalCaloriesBurned = caloriesBurned;
        Status = WorkoutStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = WorkoutStatus.Cancelled;
        EndedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

public class WorkoutExercise
{
    public Guid Id { get; private set; }
    public Guid ExerciseId { get; private set; }
    public string ExerciseName { get; private set; } = null!;
    public string? ExerciseNameVi { get; private set; }
    public string? ImageUrl { get; private set; }
    public int OrderIndex { get; private set; }
    public string? Notes { get; private set; }
    
    public List<ExerciseSet> Sets { get; private set; } = new List<ExerciseSet>();

    public WorkoutExercise() { }

    public static WorkoutExercise Create(
        Guid exerciseId,
        string name,
        string? nameVi,
        string? imageUrl,
        string? notes = null)
    {
        return new WorkoutExercise
        {
            Id = Guid.NewGuid(),
            ExerciseId = exerciseId,
            ExerciseName = name,
            ExerciseNameVi = nameVi,
            ImageUrl = imageUrl,
            Notes = notes
        };
    }

    public void AddSet(ExerciseSet set) => Sets.Add(set);
    public void SetOrderIndex(int orderIndex) => OrderIndex = orderIndex;
    public void UpdateNotes(string notes) => Notes = notes;
}

public class ExerciseSet
{
    public Guid Id { get; private set; }
    public int SetNumber { get; private set; }
    public decimal? Weight { get; private set; } // kg
    public int? Reps { get; private set; }
    public int? DurationSeconds { get; private set; }
    public decimal? Distance { get; private set; } // km for cardio
    public bool IsCompleted { get; private set; }
    public bool IsPersonalRecord { get; private set; }

    public ExerciseSet() { }

    public static ExerciseSet Create(
        int setNumber,
        decimal? weight = null,
        int? reps = null,
        int? durationSeconds = null,
        decimal? distance = null)
    {
        return new ExerciseSet
        {
            Id = Guid.NewGuid(),
            SetNumber = setNumber,
            Weight = weight,
            Reps = reps,
            DurationSeconds = durationSeconds,
            Distance = distance,
            IsCompleted = true
        };
    }

    public void UpdateWeight(decimal weight) => Weight = weight;
    public void UpdateReps(int reps) => Reps = reps;
    public void UpdateDurationSeconds(int durationSeconds) => DurationSeconds = durationSeconds;
    public void UpdateDistance(decimal distance) => Distance = distance;
    public void MarkAsPersonalRecord() => IsPersonalRecord = true;
    public void Complete() => IsCompleted = true;
}

