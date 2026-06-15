using Barkfest.Application.Features.Auth.Commands.AdminLogin;
using Barkfest.Application.Features.Auth.Commands.Login;
using Barkfest.Application.Features.Auth.Commands.Register;
using Barkfest.Application.Features.Auth.Queries.CheckDisplayName;
using Barkfest.Application.Features.Auth.Queries.CheckUsername;
using Barkfest.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Barkfest.API.Controllers;

[ApiController]
[Route("v1/auth")]
[AllowAnonymous]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.ToActionResult(id => Created($"/v1/owners/{id}", null));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.ToActionResult(token => Ok(new { token.AccountId, token.AccessToken, token.ExpiresAt }));
    }

    [HttpPost("admin/login")]
    public async Task<IActionResult> AdminLogin([FromBody] AdminLoginCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.ToActionResult(token => Ok(new { token.AccountId, token.AccessToken, token.ExpiresAt }));
    }

    [HttpGet("check-display-name")]
    public async Task<IActionResult> CheckDisplayName([FromQuery] string value, CancellationToken cancellationToken)
    {
        var available = await mediator.Send(new CheckDisplayNameQuery(value), cancellationToken);
        return Ok(new { available });
    }

    [HttpGet("check-username")]
    public async Task<IActionResult> CheckUsername([FromQuery] string value, CancellationToken cancellationToken)
    {
        var available = await mediator.Send(new CheckUsernameQuery(value), cancellationToken);
        return Ok(new { available });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Token is stateless — logout is handled client-side by discarding the token.
        // This endpoint is kept for future server-side revocation support.
        return NoContent();
    }
}
