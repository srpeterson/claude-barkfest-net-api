using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Barkfest.Persistence.Repositories;

public class OwnerRepository(AppDbContext context) : IOwnerRepository
{
    public async Task<Owner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Owners.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IEnumerable<Owner>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Owners.ToListAsync(cancellationToken);

    public async Task AddAsync(Owner owner, CancellationToken cancellationToken = default) =>
        await context.Owners.AddAsync(owner, cancellationToken);

    public Task UpdateAsync(Owner owner, CancellationToken cancellationToken = default)
    {
        context.Owners.Update(owner);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var owner = await context.Owners.FindAsync([id], cancellationToken);
        if (owner is not null)
            context.Owners.Remove(owner);
    }
}
