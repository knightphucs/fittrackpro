namespace FitTrackPro.Application.Features.Workouts.Commands.StartWorkout;

using FluentValidation;

public class StartWorkoutCommandValidator : AbstractValidator<StartWorkoutCommand>
{
    public StartWorkoutCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Workout title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters");
    }
}
