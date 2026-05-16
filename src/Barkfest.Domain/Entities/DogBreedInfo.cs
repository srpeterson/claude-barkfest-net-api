using Barkfest.Domain.Enums;
using Barkfest.Domain.Exceptions;

namespace Barkfest.Domain.Entities;

public class DogBreedInfo : Breed
{
    public DogBreed DogBreed { get; private set; } = null!;

    public void SetDogBreed(DogBreed dogBreed)
    {
        if (dogBreed is null)
            throw new DomainException("Dog breed is required.");

        DogBreed = dogBreed;
    }
}
