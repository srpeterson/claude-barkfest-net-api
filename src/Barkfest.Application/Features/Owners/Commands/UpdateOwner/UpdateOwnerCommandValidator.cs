using Barkfest.Domain.Entities;
using Barkfest.Domain.ValueObjects;
using FluentValidation;

namespace Barkfest.Application.Features.Owners.Commands.UpdateOwner;

public class UpdateOwnerCommandValidator : AbstractValidator<UpdateOwnerCommand>
{
    public UpdateOwnerCommandValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(Owner.FirstNameMaxLength);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(Owner.LastNameMaxLength);

        RuleFor(x => x.Email)
            .NotEmpty()
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("Email must be a valid email address.")
            .MaximumLength(AccountConstraints.EmailMaxLength);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(E164PhoneNumber.MaxLength)
            .WithMessage($"Phone number cannot exceed {E164PhoneNumber.MaxLength} characters.")
            .Matches(E164PhoneNumber.Pattern)
            .WithMessage("Phone number must be in E.164 format (e.g. +15555555555).")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.DisplayName)
            .MaximumLength(Owner.DisplayNameMaxLength)
            .WithMessage($"Display name cannot exceed {Owner.DisplayNameMaxLength} characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));
    }
}
