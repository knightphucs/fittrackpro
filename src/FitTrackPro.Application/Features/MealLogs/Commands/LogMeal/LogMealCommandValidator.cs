namespace FitTrackPro.Application.Features.MealLogs.Commands.LogMeal;

using FluentValidation;

public class LogMealCommandValidator : AbstractValidator<LogMealCommand>
{
    public LogMealCommandValidator()
    {
        RuleFor(x => x.FoodId)
            .NotEmpty()
            .WithMessage("Food ID is required");

        RuleFor(x => x.ServingMultiplier)
            .GreaterThan(0)
            .WithMessage("Serving multiplier must be positive")
            .LessThanOrEqualTo(50)
            .WithMessage("Serving multiplier seems too large");

        RuleFor(x => x.MealType)
            .IsInEnum()
            .WithMessage("Invalid meal type");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes must not exceed 500 characters");
    }
}
