using FluentValidation;

namespace FitTrackPro.Application.Features.Foods.Commands.UpdateFood;

public class UpdateFoodCommandValidator : AbstractValidator<UpdateFoodCommand>
{
    public UpdateFoodCommandValidator()
    {
        RuleFor(x => x.FoodId)
            .NotEmpty().WithMessage("Food ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Food name is required.")
            .MaximumLength(200).WithMessage("Food name must not exceed 200 characters.");

        RuleFor(x => x.ServingSize)
            .GreaterThan(0).WithMessage("Serving size must be greater than zero.");

        RuleFor(x => x.ServingUnit)
            .NotEmpty().WithMessage("Serving unit is required.")
            .MaximumLength(50).WithMessage("Serving unit must not exceed 50 characters.");
            
        When(x => x.Protein.HasValue || x.Carbohydrates.HasValue || x.Fats.HasValue, () =>
        {
            RuleFor(x => x.Protein)
                .NotNull().WithMessage("Protein must be provided if updating macros.")
                .GreaterThanOrEqualTo(0).WithMessage("Protein cannot be negative.");
            
            RuleFor(x => x.Carbohydrates)
                .NotNull().WithMessage("Carbohydrates must be provided if updating macros.")
                .GreaterThanOrEqualTo(0).WithMessage("Carbohydrates cannot be negative.");
            
            RuleFor(x => x.Fats)
                .NotNull().WithMessage("Fats must be provided if updating macros.")
                .GreaterThanOrEqualTo(0).WithMessage("Fats cannot be negative.");
        });

        When(x => x.Fiber.HasValue, () => 
        {
            RuleFor(x => x.Fiber!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Fiber cannot be negative.");
        });

        When(x => x.Sugar.HasValue, () => 
        {
            RuleFor(x => x.Sugar!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Sugar cannot be negative.");
        });
    }
}