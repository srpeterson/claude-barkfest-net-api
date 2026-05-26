using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using FluentValidation;

namespace Barkfest.Application.Features.Pets.Commands.CreatePet;

public class CreatePetCommandValidator : AbstractValidator<CreatePetCommand>
{
    public CreatePetCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(Pet.NameMaxLength);

        RuleFor(x => x.PetTypeValue)
            .Must(v => PetType.TryFromValue(v, out _))
            .WithMessage("Invalid pet type.");

        RuleFor(x => x.BreedValue)
            .Must((cmd, breedValue) =>
            {
                if (!PetType.TryFromValue(cmd.PetTypeValue, out var petType))
                    return true; // PetTypeValue already invalid — let that rule report the error
                return petType == PetType.Dog
                    ? DogBreed.TryFromValue(breedValue, out _)
                    : CatBreed.TryFromValue(breedValue, out _);
            })
            .WithMessage("Invalid breed.");
    }
}
