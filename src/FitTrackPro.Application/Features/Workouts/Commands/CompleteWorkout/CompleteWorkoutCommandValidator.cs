using FluentValidation;

namespace FitTrackPro.Application.Features.Workouts.Commands.CompleteWorkout;

public class CompleteWorkoutCommandValidator : AbstractValidator<CompleteWorkoutCommand>
{
    public CompleteWorkoutCommandValidator()
    {
        RuleFor(x => x.CaloriesBurned)
            .GreaterThan(0).When(x => x.CaloriesBurned.HasValue)
            .WithMessage("Calories burned must be positive");
    }
}
