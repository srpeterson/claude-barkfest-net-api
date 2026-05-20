using Ardalis.SmartEnum;

namespace Barkfest.Domain.Enums;

public sealed class CatBreed : SmartEnum<CatBreed>
{
    public static readonly CatBreed MaineCoon          = new("Maine Coon", 1);
    public static readonly CatBreed Ragdoll            = new("Ragdoll", 2);
    public static readonly CatBreed Exotic             = new("Exotic", 3);
    public static readonly CatBreed Persian            = new("Persian", 4);
    public static readonly CatBreed DevonRex           = new("Devon Rex", 5);
    public static readonly CatBreed BritishShorthair   = new("British Shorthair", 6);
    public static readonly CatBreed Abyssinian         = new("Abyssinian", 7);
    public static readonly CatBreed AmericanShorthair  = new("American Shorthair", 8);
    public static readonly CatBreed ScottishFold       = new("Scottish Fold", 9);
    public static readonly CatBreed Sphynx             = new("Sphynx", 10);
    public static readonly CatBreed Siberian           = new("Siberian", 11);
    public static readonly CatBreed RussianBlue        = new("Russian Blue", 12);
    public static readonly CatBreed Bengal             = new("Bengal", 13);
    public static readonly CatBreed Siamese            = new("Siamese", 14);
    public static readonly CatBreed NorwegianForestCat = new("Norwegian Forest Cat", 15);
    public static readonly CatBreed Birman             = new("Birman", 16);
    public static readonly CatBreed Burmese            = new("Burmese", 17);
    public static readonly CatBreed Tonkinese          = new("Tonkinese", 18);
    public static readonly CatBreed Himalayan          = new("Himalayan", 19);
    public static readonly CatBreed OrientalShorthair  = new("Oriental Shorthair", 20);
    public static readonly CatBreed Savannah           = new("Savannah", 21);
    public static readonly CatBreed Ragamuffin         = new("Ragamuffin", 22);
    public static readonly CatBreed TurkishAngora      = new("Turkish Angora", 23);
    public static readonly CatBreed Manx               = new("Manx", 24);
    public static readonly CatBreed Ocicat             = new("Ocicat", 25);
    public static readonly CatBreed DomesticShorthair  = new("Domestic Shorthair", 26);
    public static readonly CatBreed Tabby              = new("Tabby", 27);
    public static readonly CatBreed Mixed              = new("Mixed", 28);
    public static readonly CatBreed Other              = new("Other", 29);

    private CatBreed(string name, int value) : base(name, value) { }
}
