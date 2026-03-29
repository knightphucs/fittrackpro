namespace FitTrackPro.Domain.Enums;

/// <summary>
/// Types of personal records
/// </summary>
public enum PersonalRecordType
{
    MaxWeight,          // Heaviest weight lifted (1RM-style)
    MaxReps,            // Max reps at the SAME weight
    MaxVolume,          // Weight × Reps (per set or per session)
    MaxDistance,        // Cardio distance
    BestTime,           // Fastest time for a given distance
    LongestDuration     // Longest duration (plank, run, etc.)
}
