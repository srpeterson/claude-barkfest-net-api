using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Barkfest.Persistence.Repositories;

public class AdministratorRepository(AppDbContext context) : IAdministratorRepository
{
    public async Task<Administrator?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Administrators.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<Administrator?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        await context.Administrators.FirstOrDefaultAsync(a => a.Username == username.Trim(), cancellationToken);

    public async Task<Administrator?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await context.Administrators.FirstOrDefaultAsync(a => a.Email == email.Trim().ToLowerInvariant(), cancellationToken);

    public async Task AddAsync(Administrator administrator, CancellationToken cancellationToken = default) =>
        await context.Administrators.AddAsync(administrator, cancellationToken);

    public Task DeleteAsync(Administrator administrator, CancellationToken cancellationToken = default)
    {
        context.Administrators.Remove(administrator);
        return Task.CompletedTask;
    }
}
