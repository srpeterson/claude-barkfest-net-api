using Barkfest.Application.Features.Owners.Commands.DeleteOwner;
using Barkfest.Application.Features.Owners.Commands.RemoveOwnerProfileImage;
using Barkfest.Application.Features.Owners.Commands.SetOwnerVisibility;
using Barkfest.Application.Features.Owners.Commands.UpdateOwner;
using Barkfest.Application.Features.Owners.Commands.UploadOwnerProfileImage;
using Barkfest.Application.Features.Owners.Queries.GetAllOwners;
using Barkfest.Application.Features.Owners.Queries.GetOwnerById;
using Barkfest.Application.Features.Pets.Queries.GetPetsByOwnerId;
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
        var owners = await mediator.Send(new GetAllOwnersQuery(), cancellationToken);
        return Ok(owners);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var owner = await mediator.Send(new GetOwnerByIdQuery(id), cancellationToken);
        return Ok(owner);
    }

    [HttpGet("{id:guid}/pets")]
    public async Task<IActionResult> GetPets(Guid id, CancellationToken cancellationToken)
    {
        var pets = await mediator.Send(new GetPetsByOwnerIdQuery(id), cancellationToken);
        return Ok(pets);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateOwnerRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(
            new UpdateOwnerCommand(id, request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.DisplayName),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteOwnerCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/profile-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadProfileImage(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        await mediator.Send(
            new UploadOwnerProfileImageCommand(id, file.FileName, stream, file.ContentType),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}/profile-image")]
    public async Task<IActionResult> RemoveProfileImage(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new RemoveOwnerProfileImageCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/visibility")]
    public async Task<IActionResult> SetVisibility(Guid id, [FromBody] SetOwnerVisibilityRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new SetOwnerVisibilityCommand(id, request.IsVisible), cancellationToken);
        return NoContent();
    }
}

public record UpdateOwnerRequest(string FirstName, string LastName, string Email, string? PhoneNumber, string? DisplayName = null);
public record SetOwnerVisibilityRequest(bool IsVisible);
