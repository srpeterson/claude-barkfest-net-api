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
    private (string BlobName, string ContentType)? _profileImage = null;
    private readonly List<PetImage> _images = [];

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

    public PetBuilder WithProfileImage(string blobName, string contentType)
    {
        _profileImage = (blobName, contentType);
        return this;
    }

    public PetBuilder WithImage(PetImage image)
    {
        _images.Add(image);
        return this;
    }

    public Pet Build()
    {
        var pet = Pet.Create(_ownerId, _name, _petType, _description, _dateOfBirth);
        if (_profileImage.HasValue)
            pet.SetProfileImage(_profileImage.Value.BlobName, _profileImage.Value.ContentType);
        foreach (var image in _images)
            pet.AddImage(image);
        return pet;
    }
}
