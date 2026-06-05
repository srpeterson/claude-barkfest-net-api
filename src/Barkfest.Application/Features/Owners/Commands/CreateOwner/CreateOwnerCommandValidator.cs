using Barkfest.Domain.Entities;
using Barkfest.Domain.ValueObjects;
using FluentValidation;

namespace Barkfest.Application.Features.Owners.Commands.CreateOwner;

public class CreateOwnerCommandValidator : AbstractValidator<CreateOwnerCommand>
{
    public CreateOwnerCommandValidator()
    {
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
            .MaximumLength(AccountConstraints.EmailMaxLength)
            .EmailAddress().WithMessage("Email must be a valid email address.");
    }
}
