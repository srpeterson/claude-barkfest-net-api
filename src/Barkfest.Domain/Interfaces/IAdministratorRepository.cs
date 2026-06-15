using Barkfest.Domain.Entities;

namespace Barkfest.Domain.Interfaces;

public interface IAdministratorRepository
{
    Task<IEnumerable<Administrator>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Administrator?> GetByIdAsync(Guid administratorId, CancellationToken cancellationToken = default);
    Task<Administrator?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<Administrator?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(Administrator administrator, CancellationToken cancellationToken = default);
    Task DeleteAsync(Administrator administrator, CancellationToken cancellationToken = default);
}
