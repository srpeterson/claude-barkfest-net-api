using Barkfest.Domain.Entities;
using Barkfest.Domain.ValueObjects;
using FluentValidation;

namespace Barkfest.Application.Features.Owners.Commands.UpdateOwner;

public class UpdateOwnerCommandValidator : AbstractValidator<UpdateOwnerCommand>
{
    public UpdateOwnerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(Owner.FirstNameMaxLength);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(Owner.LastNameMaxLength);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(AccountConstraints.EmailMaxLength);

        RuleFor(x => x.PhoneNumber)
            .Matches(E164PhoneNumber.Pattern)
            .WithMessage("Phone number must be in E.164 format (e.g. +15555555555).")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
