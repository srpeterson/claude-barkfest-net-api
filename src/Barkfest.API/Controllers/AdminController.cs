using Barkfest.Application.Features.Administrators.Commands.CreateAdministrator;
using Barkfest.Application.Features.Administrators.Commands.DeleteAdministrator;
using Barkfest.Application.Features.Administrators.Commands.SetOwnerActive;
using Barkfest.Application.Features.Administrators.Commands.UpdateAdministratorPassword;
using Barkfest.Application.Features.Administrators.Queries.GetAllAdministrators;
using Barkfest.API.Extensions;
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
        var result = await mediator.Send(new GetAllAdministratorsQuery(), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("admins")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdministratorCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.ToActionResult(id => Created($"/v1/admin/admins/{id}", null));
    }

    [HttpPatch("admins/{administratorId:guid}/password")]
    public async Task<IActionResult> UpdateAdministratorPassword(
        Guid administratorId,
        [FromBody] UpdateAdministratorPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateAdministratorPasswordCommand(administratorId, request.NewPassword), cancellationToken);
        return result.ToNoContentResult();
    }

    [HttpDelete("admins/{administratorId:guid}")]
    public async Task<IActionResult> DeleteAdministrator(Guid administratorId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteAdministratorCommand(administratorId), cancellationToken);
        return result.ToNoContentResult();
    }

    [HttpPatch("owners/{ownerId:guid}/active")]
    public async Task<IActionResult> SetOwnerActive(
        Guid ownerId,
        [FromBody] SetOwnerActiveRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SetOwnerActiveCommand(ownerId, request.IsActive), cancellationToken);
        return result.ToNoContentResult();
    }
}

public record UpdateAdministratorPasswordRequest(string NewPassword);
public record SetOwnerActiveRequest(bool IsActive);
