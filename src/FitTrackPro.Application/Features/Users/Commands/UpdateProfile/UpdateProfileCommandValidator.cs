namespace FitTrackPro.Application.Features.Users.Commands.UpdateProfile;

using FitTrackPro.Application.Features.Users.Commands.UpdateProfile;
using FluentValidation;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Height)
            .GreaterThan(0).When(x => x.Height.HasValue)
            .WithMessage("Height must be positive");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today).When(x => x.DateOfBirth.HasValue)
            .WithMessage("Date of birth must be in the past");
    }
}