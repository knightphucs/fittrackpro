namespace FitTrackPro.Application.Features.Workouts.Commands.LogExercise;

using FluentValidation;

public class LogExerciseCommandValidator : AbstractValidator<LogExerciseCommand>
{
    public LogExerciseCommandValidator()
    {
        RuleFor(x => x.ExerciseId)
            .NotEmpty().WithMessage("Exercise ID is required");

        RuleFor(x => x.Sets)
            .NotEmpty().WithMessage("At least one set is required");

        RuleForEach(x => x.Sets).ChildRules(set =>
        {
            set.RuleFor(s => s.Weight)
                .GreaterThan(0).When(s => s.Weight.HasValue)
                .WithMessage("Weight must be positive");

            set.RuleFor(s => s.Reps)
                .GreaterThan(0).When(s => s.Reps.HasValue)
                .WithMessage("Reps must be positive");

            set.RuleFor(s => s.DurationSeconds)
                .GreaterThan(0).When(s => s.DurationSeconds.HasValue)
                .WithMessage("Duration must be positive");
        });
    }
}
