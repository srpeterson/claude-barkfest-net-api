using Barkfest.Domain.Entities;

namespace Barkfest.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateOwnerToken(Owner owner);
    string GenerateAdminToken(Administrator administrator);
    DateTime GetExpiry();
}
