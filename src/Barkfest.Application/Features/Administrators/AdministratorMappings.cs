using Barkfest.Application.Features.Administrators.DTOs;
using Barkfest.Domain.Entities;

namespace Barkfest.Application.Features.Administrators;

public static class AdministratorMappings
{
    public static AdministratorDto ToDto(this Administrator administrator) =>
        new(administrator.Id,
            administrator.Username,
            administrator.Name,
            administrator.Email,
            administrator.PhoneNumber,
            administrator.CreatedAt);

    public static IEnumerable<AdministratorDto> ToDtoList(this IEnumerable<Administrator> administrators) =>
        administrators.Select(a => a.ToDto());
}
