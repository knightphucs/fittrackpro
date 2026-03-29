using FluentValidation;

namespace FitTrackPro.Application.Features.Workouts.Commands.UpdateExerciseSet;

public class UpdateExerciseSetCommandValidator : AbstractValidator<UpdateExerciseSetCommand>
{
    public UpdateExerciseSetCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.SetId)
            .NotEmpty().WithMessage("SetId is required.");

        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(0).When(x => x.Weight.HasValue)
            .WithMessage("Weight must be non-negative.");

        RuleFor(x => x.Reps)
            .GreaterThan(0).When(x => x.Reps.HasValue)
            .WithMessage("Reps must be greater than zero.");

        RuleFor(x => x.DurationSeconds)
            .GreaterThan(0).When(x => x.DurationSeconds.HasValue)
            .WithMessage("DurationSeconds must be greater than zero.");

        RuleFor(x => x.Distance)
            .GreaterThanOrEqualTo(0).When(x => x.Distance.HasValue)
            .WithMessage("Distance must be non-negative.");
    }
}