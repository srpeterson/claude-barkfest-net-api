using Barkfest.Domain.Enums;
using Barkfest.Domain.Exceptions;

namespace Barkfest.Domain.Entities;

public class CatBreedInfo : Breed
{
    public CatBreed CatBreed { get; private set; } = null!;

    public void SetCatBreed(CatBreed catBreed)
    {
        if (catBreed is null)
            throw new DomainException("Cat breed is required.");

        CatBreed = catBreed;
    }
}
