namespace FitTrackPro.Application.Features.Progress.Commands.LogWeight;

using FluentValidation;

public class LogWeightCommandValidator : AbstractValidator<LogWeightCommand>
{
    public LogWeightCommandValidator()
    {
        RuleFor(x => x.Weight)
            .GreaterThan(0)
            .WithMessage("Weight must be positive")
            .LessThan(500)
            .WithMessage("Weight seems unrealistic");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes must not exceed 500 characters");
    }
}