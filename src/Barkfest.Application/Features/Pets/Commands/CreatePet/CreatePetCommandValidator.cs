using Barkfest.Domain.Entities;
using FluentValidation;

namespace Barkfest.Application.Features.Pets.Commands.CreatePet;

public class CreatePetCommandValidator : AbstractValidator<CreatePetCommand>
{
    public CreatePetCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(Pet.NameMaxLength);

        RuleFor(x => x.PetType)
            .NotEmpty();
    }
}
