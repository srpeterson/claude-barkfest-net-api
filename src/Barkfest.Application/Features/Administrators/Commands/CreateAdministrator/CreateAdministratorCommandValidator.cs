using Barkfest.Domain.Entities;
using FluentValidation;

namespace Barkfest.Application.Features.Administrators.Commands.CreateAdministrator;

public class CreateAdministratorCommandValidator : AbstractValidator<CreateAdministratorCommand>
{
    public CreateAdministratorCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(Administrator.EmailMaxLength)
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
