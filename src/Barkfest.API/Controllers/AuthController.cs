using Barkfest.Application.Features.Auth.Commands.AdminLogin;
using Barkfest.Application.Features.Auth.Commands.Login;
using Barkfest.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Barkfest.API.Controllers;

[ApiController]
[Route("v1/auth")]
[AllowAnonymous]
public class AuthController(IMediator mediator) : ControllerBase
{
    private const string CookieName = "barkfest_auth";

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return Created($"/v1/owners/{id}", null);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        Response.Cookies.Append(CookieName, result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure    = true,
            SameSite  = SameSiteMode.Strict,
            Expires   = result.ExpiresAt
        });

        return Ok(new { result.AccountId, result.ExpiresAt });
    }

    [HttpPost("admin/login")]
    public async Task<IActionResult> AdminLogin([FromBody] AdminLoginCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        Response.Cookies.Append(CookieName, result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure    = true,
            SameSite  = SameSiteMode.Strict,
            Expires   = result.ExpiresAt
        });

        return Ok(new { result.AccountId, result.ExpiresAt });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(CookieName);
        return NoContent();
    }
}
