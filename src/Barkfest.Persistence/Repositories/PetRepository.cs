using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Barkfest.Persistence.Repositories;

public class PetRepository(AppDbContext context) : IPetRepository
{
    public async Task<Pet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Pets
            .Include(p => p.Breed)
            .Include("_images")
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IEnumerable<Pet>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Pets
            .Include(p => p.Breed)
            .Include("_images")
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Pet>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default) =>
        await context.Pets
            .Include(p => p.Breed)
            .Include("_images")
            .Where(p => p.OwnerId == ownerId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Pet pet, CancellationToken cancellationToken = default) =>
        await context.Pets.AddAsync(pet, cancellationToken);

    public Task UpdateAsync(Pet pet, CancellationToken cancellationToken = default)
    {
        context.Pets.Update(pet);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pet = await context.Pets.FindAsync([id], cancellationToken);
        if (pet is not null)
            context.Pets.Remove(pet);
    }
}
