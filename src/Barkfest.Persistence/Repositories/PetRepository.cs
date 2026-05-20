using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Barkfest.Persistence.Repositories;

public class PetRepository(AppDbContext context) : IPetRepository
{
    public async Task<Pet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Pets
            .Include(p => p.Breed)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IEnumerable<Pet>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Pets
            .Include(p => p.Breed)
            .Include(p => p.Images)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Pet>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default) =>
        await context.Pets
            .Include(p => p.Breed)
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

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pet = await context.Pets.FindAsync([id], cancellationToken);
        if (pet is not null)
            context.Pets.Remove(pet);
    }
}
