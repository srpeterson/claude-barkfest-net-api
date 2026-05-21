using Barkfest.Application.Features.Pets.Commands.AddPetImages;
using Barkfest.Application.Features.Pets.Commands.BatchDeletePetImages;
using Barkfest.Application.Features.Pets.Commands.CreatePet;
using Barkfest.Application.Features.Pets.Commands.DeletePet;
using Barkfest.Application.Features.Pets.Commands.RemovePetImage;
using Barkfest.Application.Features.Pets.Commands.SetFeaturedImage;
using Barkfest.Application.Features.Pets.Commands.UpdatePet;
using Barkfest.Application.Features.Pets.Queries.GetPetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Barkfest.API.Controllers;

[ApiController]
[Route("v1/pets")]
[Authorize]
public class PetController(IMediator mediator) : ControllerBase
{
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
            new CreatePetCommand(request.Name, request.Description, request.DateOfBirth, request.PetType, request.Breed),
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

    [HttpPost("{id:guid}/images")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddImages(Guid id, IFormFileCollection files, CancellationToken cancellationToken)
    {
        var uploads = files
            .Select(f => new PetImageUpload(f.FileName, f.OpenReadStream(), f.ContentType))
            .ToList();

        var result = await mediator.Send(new AddPetImagesCommand(id, uploads), cancellationToken);

        if (result.Results.Any(r => !r.Success))
            return StatusCode(207, result);

        return StatusCode(201, result);
    }

    [HttpPost("{id:guid}/images/batch-delete")]
    public async Task<IActionResult> BatchDeleteImages(
        Guid id, BatchDeleteImagesRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new BatchDeletePetImagesCommand(id, request.ImageIds), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/images/{imageId:guid}/featured")]
    public async Task<IActionResult> SetFeaturedImage(Guid id, Guid imageId, CancellationToken cancellationToken)
    {
        await mediator.Send(new SetFeaturedImageCommand(id, imageId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> RemoveImage(Guid id, Guid imageId, CancellationToken cancellationToken)
    {
        await mediator.Send(new RemovePetImageCommand(id, imageId), cancellationToken);
        return NoContent();
    }
}

public record CreatePetRequest(string Name, string? Description, DateOnly? DateOfBirth, string PetType, string Breed);
public record UpdatePetRequest(string Name, string? Description, DateOnly? DateOfBirth, string PetType);
public record BatchDeleteImagesRequest(IReadOnlyList<Guid> ImageIds);
