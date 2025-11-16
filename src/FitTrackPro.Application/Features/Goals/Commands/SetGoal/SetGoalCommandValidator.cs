using FluentValidation;

namespace FitTrackPro.Application.Features.Goals.Commands.SetGoal;

public class SetGoalCommandValidator : AbstractValidator<SetGoalCommand>
{
    public SetGoalCommandValidator()
    {
        RuleFor(x => x.CurrentWeight)
            .GreaterThan(0)
            .WithMessage("Current weight must be positive");

        RuleFor(x => x.TargetWeight)
            .GreaterThan(0)
            .WithMessage("Target weight must be positive");

        RuleFor(x => x.TargetDate)
            .GreaterThan(DateTime.Today).When(x => x.TargetDate.HasValue)
            .WithMessage("Target date must be in the future");

        RuleFor(x => x.ActivityLevel)
            .IsInEnum()
            .WithMessage("Invalid activity level");

        RuleFor(x => x.WeightGoal)
            .IsInEnum()
            .WithMessage("Invalid weight goal");
    }
}