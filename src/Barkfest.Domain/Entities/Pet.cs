using Barkfest.Domain.Enums;
using Barkfest.Domain.Exceptions;

namespace Barkfest.Domain.Entities;

public class Pet
{
    public const int NameMaxLength = 75;
    public const int MaxImages = 6;

    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid OwnerId { get; private set; }
    public Owner Owner { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public PetType PetType { get; private set; } = null!;
    public Breed? Breed { get; private set; }
    public IReadOnlyCollection<PetImage> Images => _images.AsReadOnly();
    public PetImage? FeaturedImage => _images.FirstOrDefault(i => i.IsFeaturedImage);
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public int? Age => DateOfBirth.HasValue ? CalculateAge(DateOfBirth.Value) : null;

    private readonly List<PetImage> _images = [];

    private Pet() { }

    public Pet(Guid ownerId)
    {
        OwnerId = ownerId;
    }

    public static Pet Create(
        Guid ownerId,
        string name,
        PetType petType,
        Breed breed,
        string? description = null,
        DateOnly? dateOfBirth = null)
    {
        var pet = new Pet(ownerId);
        pet.SetName(name);
        pet.SetPetType(petType);
        pet.SetBreed(breed);
        pet.SetDescription(description);
        pet.SetDateOfBirth(dateOfBirth);
        return pet;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");

        if (name.Trim().Length > NameMaxLength)
            throw new DomainException($"Name cannot exceed {NameMaxLength} characters.");

        Name = name.Trim();
    }

    public void SetDescription(string? description) =>
        Description = description?.Trim();

    public void SetDateOfBirth(DateOnly? dateOfBirth)
    {
        if (dateOfBirth.HasValue && dateOfBirth.Value > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new DomainException("Date of birth cannot be in the future.");

        DateOfBirth = dateOfBirth;
    }

    public void SetPetType(PetType petType)
    {
        if (petType is null)
            throw new DomainException("Pet type is required.");

        PetType = petType;
    }

    public void SetBreed(Breed breed)
    {
        if (breed is null)
            throw new DomainException("Breed is required.");

        if (PetType == PetType.Dog && breed is not DogBreedInfo)
            throw new DomainException("Dog pet type requires a dog breed.");

        if (PetType == PetType.Cat && breed is not CatBreedInfo)
            throw new DomainException("Cat pet type requires a cat breed.");

        Breed = breed;
    }

    public void SetFeaturedImage(Guid petImageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == petImageId)
            ?? throw new DomainException("Image not found.");

        foreach (var img in _images.Where(i => i.IsFeaturedImage))
            img.UnsetAsFeatured();

        image.SetAsFeatured();
    }

    public void AddImage(PetImage image)
    {
        if (image is null)
            throw new DomainException("Image is required.");

        if (_images.Count >= MaxImages)
            throw new DomainException($"A pet cannot have more than {MaxImages} images.");

        if (_images.Count == 0)
            image.SetAsFeatured();

        _images.Add(image);
    }

    public void RemoveImage(Guid petImageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == petImageId);

        if (image is null)
            throw new DomainException("Image not found.");

        _images.Remove(image);
    }

    public void RemoveImages(IReadOnlyList<Guid> petImageIds)
    {
        if (petImageIds is null || petImageIds.Count == 0)
            throw new DomainException("At least one image ID is required.");

        var notFound = petImageIds.Where(id => _images.All(i => i.Id != id)).ToList();
        if (notFound.Count > 0)
            throw new DomainException("One or more images were not found.");

        var toRemove = _images.Where(i => petImageIds.Contains(i.Id)).ToList();
        foreach (var image in toRemove)
            _images.Remove(image);
    }

    private static int CalculateAge(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.AddYears(age) > today)
            age--;
        return age;
    }
}
