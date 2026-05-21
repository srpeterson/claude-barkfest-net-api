using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;

namespace Barkfest.Tests.Common.Builders;

public class PetBuilder
{
    private Guid _ownerId = Guid.NewGuid();
    private string _name = "Buddy";
    private string? _description = null;
    private DateOnly? _dateOfBirth = null;
    private PetType _petType = PetType.Dog;
    private Breed _breed = DefaultDogBreed();
    private readonly List<PetImage> _images = [];

    private static Breed DefaultDogBreed()
    {
        var breed = new DogBreedInfo();
        breed.SetDogBreed(DogBreed.Beagle);
        return breed;
    }

    public PetBuilder WithOwnerId(Guid ownerId)
    {
        _ownerId = ownerId;
        return this;
    }

    public PetBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public PetBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public PetBuilder WithDateOfBirth(DateOnly? dateOfBirth)
    {
        _dateOfBirth = dateOfBirth;
        return this;
    }

    public PetBuilder WithPetType(PetType petType)
    {
        _petType = petType;
        return this;
    }

    public PetBuilder WithBreed(Breed breed)
    {
        ArgumentNullException.ThrowIfNull(breed);
        _breed = breed;
        return this;
    }

    public PetBuilder WithImage(PetImage image)
    {
        _images.Add(image);
        return this;
    }

    public Pet Build()
    {
        var pet = Pet.Create(_ownerId, _name, _petType, _breed, _description, _dateOfBirth);
        foreach (var image in _images)
            pet.AddImage(image);
        return pet;
    }
}
