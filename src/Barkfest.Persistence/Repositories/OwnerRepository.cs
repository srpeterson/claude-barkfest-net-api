using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Barkfest.Persistence.Repositories;

public class OwnerRepository(AppDbContext context) : IOwnerRepository
{
    public async Task<Owner?> GetByIdAsync(Guid ownerId, CancellationToken cancellationToken = default) =>
        await context.Owners.FirstOrDefaultAsync(o => o.Id == ownerId, cancellationToken);

    public async Task<Owner?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        await context.Owners.FirstOrDefaultAsync(o => o.Username == username.Trim(), cancellationToken);

    public async Task<Owner?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await context.Owners.FirstOrDefaultAsync(o => o.Email == email.Trim().ToLowerInvariant(), cancellationToken);

    public async Task<bool> IsDisplayNameAvailableAsync(
        string normalizedValue, Guid? excludeOwnerId = null, CancellationToken cancellationToken = default)
    {
        // Indexed equality against the persisted normalized column (filtered unique index) -
        // an index seek, not a full scan. The caller passes the already-normalized value.
        // excludeOwnerId lets an owner keep their own display name on update.
        var isTaken = await context.Owners
            .AnyAsync(
                o => o.DisplayNameNormalized == normalizedValue
                     && (excludeOwnerId == null || o.Id != excludeOwnerId),
                cancellationToken);

        return !isTaken;
    }

    public async Task<bool> IsUsernameAvailableAsync(string username, CancellationToken cancellationToken = default) =>
        !await context.Owners.AnyAsync(o => o.Username == username.Trim(), cancellationToken);

    public async Task<IEnumerable<Owner>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Owners.ToListAsync(cancellationToken);

    public async Task AddAsync(Owner owner, CancellationToken cancellationToken = default) =>
        await context.Owners.AddAsync(owner, cancellationToken);

    public Task UpdateAsync(Owner owner, CancellationToken cancellationToken = default)
    {
        context.Owners.Update(owner);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var owner = await context.Owners.FindAsync([ownerId], cancellationToken);
        if (owner is not null)
            context.Owners.Remove(owner);
    }
}
