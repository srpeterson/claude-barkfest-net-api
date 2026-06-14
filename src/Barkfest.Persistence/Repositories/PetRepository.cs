using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Barkfest.Persistence.Repositories;

public class PetRepository(AppDbContext context) : IPetRepository
{
    public async Task<Pet?> GetByIdAsync(Guid petId, CancellationToken cancellationToken = default) =>
        await context.Pets
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == petId, cancellationToken);

    public async Task<IEnumerable<Pet>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Pets
            .Include(p => p.Images)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Pet>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default) =>
        await context.Pets
            .Include(p => p.Images)
            .Where(p => p.OwnerId == ownerId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Pet pet, CancellationToken cancellationToken = default) =>
        await context.Pets.AddAsync(pet, cancellationToken);

    public Task UpdateAsync(Pet pet, CancellationToken cancellationToken = default)
    {
        // Root cause: HasDefaultValueSql("newsequentialid()") implies ValueGeneratedOnAdd,
        // so EF Core uses Guid.Empty as the "new entity" sentinel. Because PetImage.Id is
        // initialised to Guid.CreateVersion7() (non-empty), EF Core treats every new image
        // as an *existing* row and marks it Modified instead of Added.
        //
        // Compounding problem: both context.Entry(entity) and context.ChangeTracker.Entries()
        // trigger AutoDetectChanges, which starts tracking new PetImages (found in the
        // pet._images backing field) *with Modified state* before we have a chance to
        // identify them as new. Disabling AutoDetectChanges prevents that premature
        // fixup so we can correctly snapshot which images are truly Detached (= new).
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        try
        {
            // With auto-detect off, context.Entry() does not trigger DetectChanges.
            // Any image still Detached at this point has never been persisted.
            var newImages = pet.Images
                .Where(i => context.Entry(i).State == EntityState.Detached)
                .ToList();

            // Update(pet) marks the Pet and all reachable entities as Modified. This is
            // correct for scalar-property changes and the owned ProfileImage. New PetImages
            // incorrectly become Modified here (see above); we correct that below.
            context.Pets.Update(pet);

            // Re-promote new PetImages from Modified → Added so EF Core emits INSERT.
            // Existing images remain Modified (harmless — data is unchanged).
            // Images removed via pet.RemoveImage() are no longer reachable from the graph;
            // DetectChanges (run by SaveChanges after auto-detect is re-enabled) detects
            // the orphaned required-FK dependents and marks them Deleted automatically.
            foreach (var image in newImages)
                context.Entry(image).State = EntityState.Added;
        }
        finally
        {
            // Restore so SaveChangesAsync can run DetectChanges normally, enabling
            // orphan detection for removed images and scalar-change tracking.
            context.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        var pet = await context.Pets.FindAsync([petId], cancellationToken);
        if (pet is not null)
            context.Pets.Remove(pet);
    }

    public async Task<LikeUpdateResult> IncrementLikesAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        // Single atomic statement: UPDATE Pets SET Likes = Likes + 1 WHERE PetId = @petId.
        // The database performs the increment under its own row lock, so concurrent
        // likes serialise correctly and never lose an update. Runs immediately and
        // bypasses the change tracker / IUnitOfWork by design.
        var rowsAffected = await context.Pets
            .Where(p => p.Id == petId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Likes, p => p.Likes + 1), cancellationToken);

        return await BuildResultAsync(petId, rowsAffected, cancellationToken);
    }

    public async Task<LikeUpdateResult> DecrementLikesAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        // Atomic decrement that floors at zero via a server-side CASE expression,
        // mirroring Pet.DecrementLikes(). Same concurrency guarantees as the increment.
        var rowsAffected = await context.Pets
            .Where(p => p.Id == petId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(p => p.Likes, p => p.Likes > 0 ? p.Likes - 1 : 0),
                cancellationToken);

        return await BuildResultAsync(petId, rowsAffected, cancellationToken);
    }

    // rowsAffected == 0 means no pet matched the id. Otherwise read back the new count.
    // The read-back is advisory: the stored value is always correct, but it may reflect
    // other concurrent likes, which is acceptable for a public display counter (the UI
    // owns liked state via localStorage).
    private async Task<LikeUpdateResult> BuildResultAsync(Guid petId, int rowsAffected, CancellationToken cancellationToken)
    {
        if (rowsAffected == 0)
            return new LikeUpdateResult(PetExists: false, Likes: 0);

        var likes = await context.Pets
            .Where(p => p.Id == petId)
            .Select(p => p.Likes)
            .FirstAsync(cancellationToken);

        return new LikeUpdateResult(PetExists: true, likes);
    }
}
