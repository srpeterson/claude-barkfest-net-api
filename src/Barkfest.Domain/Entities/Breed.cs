namespace Barkfest.Domain.Entities;

public abstract class Breed
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid PetId { get; private set; }
    public Pet Pet { get; private set; } = null!;
}
