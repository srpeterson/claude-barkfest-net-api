using Barkfest.Application.Features.Pets.Commands.AddPetImage;
using Barkfest.Application.Features.Pets.Commands.CreatePet;
using Barkfest.Application.Features.Pets.Commands.DeletePet;
using Barkfest.Application.Features.Pets.Commands.RemovePetImage;
using Barkfest.Application.Features.Pets.Commands.RemovePetProfileImage;
using Barkfest.Application.Features.Pets.Commands.UpdatePet;
using Barkfest.Application.Features.Pets.Commands.UploadPetProfileImage;
using Barkfest.Application.Features.Pets.Queries.GetAllPets;
using Barkfest.Application.Features.Pets.Queries.GetPetById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Barkfest.API.Controllers;

[ApiController]
[Route("v1/pets")]
public class PetController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var pets = await mediator.Send(new GetAllPetsQuery(), cancellationToken);
        return Ok(pets);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var pet = await mediator.Send(new GetPetByIdQuery(id), cancellationToken);
        return Ok(pet);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePetRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(
            new CreatePetCommand(request.OwnerId, request.Name, request.Description, request.DateOfBirth, request.PetType),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdatePetRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(
            new UpdatePetCommand(id, request.Name, request.Description, request.DateOfBirth, request.PetType),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeletePetCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/profile-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadProfileImage(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        await mediator.Send(
            new UploadPetProfileImageCommand(id, file.FileName, stream, file.ContentType),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}/profile-image")]
    public async Task<IActionResult> RemoveProfileImage(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new RemovePetProfileImageCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/images")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddImage(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var imageId = await mediator.Send(
            new AddPetImageCommand(id, file.FileName, stream, file.ContentType),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, new { imageId });
    }

    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> RemoveImage(Guid id, Guid imageId, CancellationToken cancellationToken)
    {
        await mediator.Send(new RemovePetImageCommand(id, imageId), cancellationToken);
        return NoContent();
    }
}

public record CreatePetRequest(Guid OwnerId, string Name, string? Description, DateOnly? DateOfBirth, string PetType);
public record UpdatePetRequest(string Name, string? Description, DateOnly? DateOfBirth, string PetType);
