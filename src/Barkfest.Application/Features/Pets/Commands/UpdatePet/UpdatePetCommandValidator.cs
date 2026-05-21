using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using FluentValidation;

namespace Barkfest.Application.Features.Pets.Commands.UpdatePet;

public class UpdatePetCommandValidator : AbstractValidator<UpdatePetCommand>
{
    public UpdatePetCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(Pet.NameMaxLength);

        RuleFor(x => x.PetType)
            .NotEmpty()
            .Must(pt => PetType.List.Any(p => p.Name == pt))
            .WithMessage("Pet type must be 'Dog' or 'Cat'.");
    }
}
