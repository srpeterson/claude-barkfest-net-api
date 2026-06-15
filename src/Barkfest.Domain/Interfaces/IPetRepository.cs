using Barkfest.Domain.Entities;

namespace Barkfest.Domain.Interfaces;

public interface IPetRepository
{
    Task<Pet?> GetByIdAsync(Guid petId, CancellationToken cancellationToken = default);

    // Read-only fetch that eagerly loads the pet's Images AND its Owner in a single query,
    // with change tracking disabled. Used by the public GetPetById path, which needs the
    // owner's IsActive/IsVisible flags for the visibility checks. Deliberately separate from
    // GetByIdAsync: the tracking GetByIdAsync feeds the mutation handlers, where loading Owner
    // would cause context.Pets.Update(pet) to mark the Owner row Modified too.
    Task<Pet?> GetByIdWithOwnerAsync(Guid petId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Pet>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Pet>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task AddAsync(Pet pet, CancellationToken cancellationToken = default);
    Task UpdateAsync(Pet pet, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid petId, CancellationToken cancellationToken = default);

    // Atomic relative updates for the public like counter. These bypass the change
    // tracker and IUnitOfWork by design (see CLAUDE.md) so concurrent likes cannot
    // lose updates. PetExists is false when no pet matched the id.
    Task<LikeUpdateResult> IncrementLikesAsync(Guid petId, CancellationToken cancellationToken = default);
    Task<LikeUpdateResult> DecrementLikesAsync(Guid petId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Outcome of an atomic like-counter update. <see cref="PetExists"/> is false when no
/// pet matched the id (the caller maps this to a not-found result); otherwise
/// <see cref="Likes"/> is the new advisory count.
/// </summary>
public readonly record struct LikeUpdateResult(bool PetExists, int Likes);
