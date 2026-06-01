using Barkfest.Domain.Entities;

namespace Barkfest.Domain.Interfaces;

public interface IOwnerRepository
{
    Task<Owner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Owner?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<Owner?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> IsDisplayNameAvailableAsync(string normalizedValue, CancellationToken cancellationToken = default);
    Task<bool> IsUsernameAvailableAsync(string username, CancellationToken cancellationToken = default);
    Task<IEnumerable<Owner>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Owner owner, CancellationToken cancellationToken = default);
    Task UpdateAsync(Owner owner, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
