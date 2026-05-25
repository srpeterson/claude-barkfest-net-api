using Ardalis.SmartEnum;

namespace Barkfest.Domain.Enums;

public sealed class DogBreed : SmartEnum<DogBreed>
{
    public static readonly DogBreed FrenchBulldog            = new("French Bulldog", 1);
    public static readonly DogBreed LabradorRetriever         = new("Labrador Retriever", 2);
    public static readonly DogBreed GoldenRetriever           = new("Golden Retriever", 3);
    public static readonly DogBreed GermanShepherdDog         = new("German Shepherd Dog", 4);
    public static readonly DogBreed Dachshund                 = new("Dachshund", 5);
    public static readonly DogBreed Poodle                    = new("Poodle", 6);
    public static readonly DogBreed Beagle                    = new("Beagle", 7);
    public static readonly DogBreed Rottweiler                = new("Rottweiler", 8);
    public static readonly DogBreed GermanShorthairedPointer  = new("German Shorthaired Pointer", 9);
    public static readonly DogBreed Bulldog                   = new("Bulldog", 10);
    public static readonly DogBreed CaneCorsо                 = new("Cane Corso", 11);
    public static readonly DogBreed CockerSpaniel               = new("Cocker Spaniel", 12);
    public static readonly DogBreed YorkshireTerrier          = new("Yorkshire Terrier", 13);
    public static readonly DogBreed AustralianShepherd        = new("Australian Shepherd", 14);
    public static readonly DogBreed DobermanPinscher          = new("Doberman Pinscher", 15);
    public static readonly DogBreed PembrokeWelshCorgi        = new("Pembroke Welsh Corgi", 16);
    public static readonly DogBreed MiniatureSchnauzer        = new("Miniature Schnauzer", 17);
    public static readonly DogBreed Boxer                     = new("Boxer", 18);
    public static readonly DogBreed Pomeranian                = new("Pomeranian", 19);
    public static readonly DogBreed BerneseMountainDog        = new("Bernese Mountain Dog", 20);
    public static readonly DogBreed ShihTzu                   = new("Shih Tzu", 21);
    public static readonly DogBreed GreatDane                 = new("Great Dane", 22);
    public static readonly DogBreed BostonTerrier             = new("Boston Terrier", 23);
    public static readonly DogBreed Chihuahua                 = new("Chihuahua", 24);
    public static readonly DogBreed Havanese                  = new("Havanese", 25);
    public static readonly DogBreed Labradoodle               = new("Labradoodle", 26);
    public static readonly DogBreed Goldendoodle              = new("Goldendoodle", 27);
    public static readonly DogBreed Cockapoo                  = new("Cockapoo", 28);
    public static readonly DogBreed SaintBernard              = new("St. Bernard", 31);
    public static readonly DogBreed Mixed                     = new("Mixed", 29);
    public static readonly DogBreed Other                     = new("Other", 30);

    private DogBreed(string name, int value) : base(name, value) { }
}
