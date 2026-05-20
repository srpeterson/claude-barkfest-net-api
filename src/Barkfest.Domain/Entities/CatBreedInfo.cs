using Barkfest.Domain.Enums;
using Barkfest.Domain.Exceptions;

namespace Barkfest.Domain.Entities;

public class CatBreedInfo : Breed
{
    public CatBreed CatBreed { get; private set; } = null!;

    public static CatBreedInfo Create(CatBreed catBreed)
    {
        var breed = new CatBreedInfo();
        breed.SetCatBreed(catBreed);
        return breed;
    }

    public void SetCatBreed(CatBreed catBreed)
    {
        if (catBreed is null)
            throw new DomainException("Cat breed is required.");

        CatBreed = catBreed;
    }
}
