using Barkfest.Application.Features.Owners.Commands.ChangeOwnerPassword;
using Barkfest.Application.Features.Owners.Commands.DeleteOwner;
using Barkfest.Application.Features.Owners.Commands.RemoveOwnerProfileImage;
using Barkfest.Application.Features.Owners.Commands.SetOwnerVisibility;
using Barkfest.Application.Features.Owners.Commands.UpdateOwner;
using Barkfest.Application.Features.Owners.Commands.UploadOwnerProfileImage;
using Barkfest.Application.Features.Owners.Queries.GetAllOwners;
using Barkfest.Application.Features.Owners.Queries.GetOwnerById;
using Barkfest.Application.Features.Pets.Queries.GetPetsByOwnerId;
using Barkfest.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Barkfest.API.Controllers;

[ApiController]
[Route("v1/owners")]
[Authorize]
public class OwnerController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllOwnersQuery(), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{ownerId:guid}")]
    public async Task<IActionResult> GetById(Guid ownerId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOwnerByIdQuery(ownerId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{ownerId:guid}/pets")]
    public async Task<IActionResult> GetPets(Guid ownerId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPetsByOwnerIdQuery(ownerId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{ownerId:guid}")]
    public async Task<IActionResult> Update(Guid ownerId, UpdateOwnerRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateOwnerCommand(ownerId, request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.DisplayName),
            cancellationToken);

        return result.ToNoContentResult();
    }

    [HttpDelete("{ownerId:guid}")]
    public async Task<IActionResult> Delete(Guid ownerId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteOwnerCommand(ownerId), cancellationToken);
        return result.ToNoContentResult();
    }

    [HttpPost("{ownerId:guid}/profile-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadProfileImage(Guid ownerId, IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var result = await mediator.Send(
            new UploadOwnerProfileImageCommand(ownerId, file.FileName, stream, file.ContentType),
            cancellationToken);

        return result.ToNoContentResult();
    }

    [HttpDelete("{ownerId:guid}/profile-image")]
    public async Task<IActionResult> RemoveProfileImage(Guid ownerId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RemoveOwnerProfileImageCommand(ownerId), cancellationToken);
        return result.ToNoContentResult();
    }

    [HttpPut("{ownerId:guid}/password")]
    public async Task<IActionResult> ChangePassword(Guid ownerId, [FromBody] ChangeOwnerPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ChangeOwnerPasswordCommand(ownerId, request.CurrentPassword, request.NewPassword), cancellationToken);
        return result.ToNoContentResult();
    }

    [HttpPatch("{ownerId:guid}/visibility")]
    public async Task<IActionResult> SetVisibility(Guid ownerId, [FromBody] SetOwnerVisibilityRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SetOwnerVisibilityCommand(ownerId, request.IsVisible), cancellationToken);
        return result.ToNoContentResult();
    }
}

public record UpdateOwnerRequest(string FirstName, string LastName, string Email, string? PhoneNumber, string? DisplayName = null);
public record ChangeOwnerPasswordRequest(string CurrentPassword, string NewPassword);
public record SetOwnerVisibilityRequest(bool IsVisible);
