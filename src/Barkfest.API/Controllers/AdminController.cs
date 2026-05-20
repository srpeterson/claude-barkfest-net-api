using Barkfest.Application.Features.Administrators.Commands.CreateAdministrator;
using Barkfest.Application.Features.Administrators.Commands.DeleteAdministrator;
using Barkfest.Application.Features.Administrators.Commands.SetOwnerActive;
using Barkfest.Application.Features.Administrators.Commands.UpdateAdministratorPassword;
using Barkfest.Application.Features.Administrators.Queries.GetAllAdministrators;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Barkfest.API.Controllers;

[ApiController]
[Route("v1/admin")]
[Authorize]
public class AdminController(IMediator mediator) : ControllerBase
{
    [HttpGet("admins")]
    public async Task<IActionResult> GetAllAdministrators(CancellationToken cancellationToken)
    {
        var administrators = await mediator.Send(new GetAllAdministratorsQuery(), cancellationToken);
        return Ok(administrators);
    }

    [HttpPost("admins")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdministratorCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return Created($"/v1/admin/admins/{id}", null);
    }

    [HttpPatch("admins/{id:guid}/password")]
    public async Task<IActionResult> UpdateAdministratorPassword(
        Guid id,
        [FromBody] UpdateAdministratorPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateAdministratorPasswordCommand(id, request.NewPassword), cancellationToken);
        return NoContent();
    }

    [HttpDelete("admins/{id:guid}")]
    public async Task<IActionResult> DeleteAdministrator(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteAdministratorCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPatch("owners/{id:guid}/active")]
    public async Task<IActionResult> SetOwnerActive(
        Guid id,
        [FromBody] SetOwnerActiveRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new SetOwnerActiveCommand(id, request.Active), cancellationToken);
        return NoContent();
    }
}

public record UpdateAdministratorPasswordRequest(string NewPassword);
public record SetOwnerActiveRequest(bool Active);
