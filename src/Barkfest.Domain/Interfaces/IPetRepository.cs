using Barkfest.Domain.Entities;

namespace Barkfest.Domain.Interfaces;

public interface IPetRepository
{
    Task<Pet?> GetByIdAsync(Guid petId, CancellationToken cancellationToken = default);
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
