using FluentValidation;

namespace FitTrackPro.Application.Features.Progress.Commands.LogMeasurements;

public class LogMeasurementsValidator : AbstractValidator<LogMeasurementsCommand>
{
    public LogMeasurementsValidator()
    {
        RuleFor(x => x.Weight)
            .GreaterThan(0)
            .WithMessage("Weight must be positive")
            .LessThan(500)
            .WithMessage("Weight seems unrealistic");

        RuleFor(x => x.BodyFatPercentage)
            .InclusiveBetween(0, 100)
            .WithMessage("Body fat percentage must be between 0 and 100");

        RuleFor(x => x.Chest)
            .GreaterThan(0)
            .WithMessage("Chest measurement must be positive");

        RuleFor(x => x.Waist)
            .GreaterThan(0)
            .WithMessage("Waist measurement must be positive");

        RuleFor(x => x.Hips)
            .GreaterThan(0)
            .WithMessage("Hips measurement must be positive");

        RuleFor(x => x.Arms)
            .GreaterThan(0)
            .WithMessage("Arms measurement must be positive");

        RuleFor(x => x.Legs)
            .GreaterThan(0)
            .WithMessage("Legs measurement must be positive");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes must not exceed 500 characters");
    }
}