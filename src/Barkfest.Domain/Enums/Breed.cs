namespace Barkfest.Domain.Enums;

/// <summary>
/// The single home for the <see cref="PetType"/> → breed-enum mapping. <see cref="DogBreed"/>
/// and <see cref="CatBreed"/> are independent SmartEnum types with no shared base, so every
/// breed operation must fork on the pet's type. This class is the one place that fork lives -
/// adding a future species means editing only here, not every call site.
/// </summary>
public static class Breed
{
    public static bool IsValid(PetType petType, int value) =>
        petType == PetType.Dog
            ? DogBreed.TryFromValue(value, out _)
            : CatBreed.TryFromValue(value, out _);

    public static string NameFor(PetType petType, int value) =>
        petType == PetType.Dog
            ? DogBreed.FromValue(value).Name
            : CatBreed.FromValue(value).Name;

    public static IReadOnlyList<BreedOption> ListFor(PetType petType) =>
        petType == PetType.Dog
            ? DogBreed.List.OrderBy(b => b.Value).Select(b => new BreedOption(b.Value, b.Name)).ToList()
            : CatBreed.List.OrderBy(b => b.Value).Select(b => new BreedOption(b.Value, b.Name)).ToList();
}

/// <summary>A breed's value and display name, decoupled from the concrete SmartEnum type.</summary>
public readonly record struct BreedOption(int Value, string Name);
