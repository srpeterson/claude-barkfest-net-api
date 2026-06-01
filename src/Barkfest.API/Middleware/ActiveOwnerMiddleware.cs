using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Barkfest.Domain.Interfaces;

namespace Barkfest.API.Middleware;

public class ActiveOwnerMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IOwnerRepository ownerRepository)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Admin tokens have no owner account — skip the owner active check entirely.
            var accountType = context.User.FindFirstValue("account_type");
            if (accountType == "admin")
            {
                await next(context);
                return;
            }

            var sub = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (Guid.TryParse(sub, out var ownerId))
            {
                var owner = await ownerRepository.GetByIdAsync(ownerId, context.RequestAborted);

                if (owner is not null && !owner.IsActive)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
            }
        }

        await next(context);
    }
}
