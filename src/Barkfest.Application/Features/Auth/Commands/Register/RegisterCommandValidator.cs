using Barkfest.Domain.Entities;
using FluentValidation;

namespace Barkfest.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(Owner.UsernameMaxLength)
            .WithMessage($"Username cannot exceed {Owner.UsernameMaxLength} characters.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(Owner.FirstNameMaxLength)
            .WithMessage($"First name cannot exceed {Owner.FirstNameMaxLength} characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(Owner.LastNameMaxLength)
            .WithMessage($"Last name cannot exceed {Owner.LastNameMaxLength} characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(Owner.EmailMaxLength)
            .WithMessage($"Email cannot exceed {Owner.EmailMaxLength} characters.")
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
