using FluentValidation;

namespace FitTrackPro.Application.Features.Foods.Commands.CreateFood;

public class CreateFoodCommandValidator : AbstractValidator<CreateFoodCommand>
{
    public CreateFoodCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Food name is required.")
            .MaximumLength(200)
            .WithMessage("Food name must not exceed 200 characters.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(100)
            .WithMessage("Category must not exceed 100 characters.");

        RuleFor(x => x.ServingSize)
            .GreaterThan(0)
            .WithMessage("Serving size must be greater than zero.");

        RuleFor(x => x.ServingUnit)
            .NotEmpty().WithMessage("Serving unit is required.")
            .MaximumLength(50).WithMessage("Serving unit must not exceed 50 characters.");

        RuleFor(x => x.Calories)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Calories must be a non-negative value.");

        RuleFor(x => x.Protein)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Protein must be a non-negative value.");

        RuleFor(x => x.Carbohydrates)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Carbohydrates must be a non-negative value.");

        RuleFor(x => x.Fats)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Fats must be a non-negative value.");

        When(v => v.Fiber.HasValue, () =>
        {
            RuleFor(v => v.Fiber!.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Fiber must be a non-negative value.");
        });

        When(v => v.Sugar.HasValue, () =>
        {
            RuleFor(v => v.Sugar!.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Sugar must be a non-negative value.");
        });

        When(v => v.ImageFile != null, () =>
        {
            RuleFor(v => v.ImageFile!.Length).LessThan(5 * 1024 * 1024)
                .WithMessage("Image size must be less than 5MB.");

            RuleFor(v => v.ImageFile!.ContentType).Must(x => 
                x.Equals("image/jpeg") || x.Equals("image/jpg") || x.Equals("image/png") || x.Equals("image/webp"))
                .WithMessage("File type is not allowed. Please upload JPG, PNG or WEBP.");
        });
    }
}