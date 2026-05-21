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

        RuleFor(x => x.PetType)
            .NotEmpty()
            .Must(pt => PetType.List.Any(p => p.Name == pt))
            .WithMessage("Pet type must be 'Dog' or 'Cat'.");

        RuleFor(x => x.Breed)
            .NotEmpty().WithMessage("Breed is required.")
            .When(x => x.PetType == "Dog" || x.PetType == "Cat");

        RuleFor(x => x.Breed)
            .Must(b => DogBreed.List.Any(d => d.Name == b))
            .WithMessage("Invalid dog breed.")
            .When(x => x.PetType == "Dog" && !string.IsNullOrWhiteSpace(x.Breed));

        RuleFor(x => x.Breed)
            .Must(b => CatBreed.List.Any(c => c.Name == b))
            .WithMessage("Invalid cat breed.")
            .When(x => x.PetType == "Cat" && !string.IsNullOrWhiteSpace(x.Breed));
    }
}
