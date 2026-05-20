using Ardalis.SmartEnum;

namespace Barkfest.Domain.Enums;

public sealed class PetType : SmartEnum<PetType>
{
    public static readonly PetType Dog = new(nameof(Dog), 1);
    public static readonly PetType Cat = new(nameof(Cat), 2);
    public static readonly PetType Other = new(nameof(Other), 3);

    private PetType(string name, int value) : base(name, value) { }
}
