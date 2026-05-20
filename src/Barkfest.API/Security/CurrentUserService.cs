using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Barkfest.Application.Common.Interfaces;

namespace Barkfest.API.Security;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? OwnerId
    {
        get
        {
            if (IsAdmin) return null;
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public Guid? AdminId
    {
        get
        {
            if (!IsAdmin) return null;
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public bool IsAdmin =>
        httpContextAccessor.HttpContext?.User.FindFirstValue("account_type") == "admin";
}
