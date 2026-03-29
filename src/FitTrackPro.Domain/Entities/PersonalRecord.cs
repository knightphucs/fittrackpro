namespace FitTrackPro.Domain.Entities;

using FitTrackPro.Domain.Common;
using FitTrackPro.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class PersonalRecord
{
    [BsonId]
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ExerciseId { get; private set; }
    public string ExerciseName { get; private set; } = default!;

    [BsonRepresentation(BsonType.String)]
    public PersonalRecordType Type { get; private set; }

    public decimal Value { get; private set; }
    public decimal? RelatedWeight { get; private set; }
    public string Unit { get; private set; } = default!; // kg, reps, seconds
    public DateTime AchievedAt { get; private set; }
    public Guid? WorkoutSessionId { get; private set; }

    public PersonalRecord() { }

    public static PersonalRecord Create(
        Guid userId,
        Guid exerciseId,
        string exerciseName,
        PersonalRecordType type,
        decimal value,
        string unit,
        DateTime achievedAt,
        Guid? workoutSessionId = null,
        decimal? relatedWeight = null)
    {
        return new PersonalRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExerciseId = exerciseId,
            ExerciseName = exerciseName,
            Type = type,
            Value = value,
            Unit = unit,
            RelatedWeight = relatedWeight,
            AchievedAt = achievedAt,
            WorkoutSessionId = workoutSessionId
        };
    }
}
