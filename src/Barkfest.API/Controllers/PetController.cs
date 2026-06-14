using Barkfest.Application.Features.Pets.Commands.AddPetImages;
using Barkfest.Application.Features.Pets.Commands.BatchDeletePetImages;
using Barkfest.Application.Features.Pets.Commands.CreatePet;
using Barkfest.Application.Features.Pets.Commands.DecrementPetLikes;
using Barkfest.Application.Features.Pets.Commands.DeletePet;
using Barkfest.Application.Features.Pets.Commands.IncrementPetLikes;
using Barkfest.Application.Features.Pets.Commands.RemovePetImage;
using Barkfest.Application.Features.Pets.Commands.SetFeaturedImage;
using Barkfest.Application.Features.Pets.Commands.UpdatePet;
using Barkfest.Application.Features.Pets.Queries.GetPetById;
using Barkfest.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Barkfest.API.Controllers;

[ApiController]
[Route("v1/pets")]
[Authorize]
public class PetController(IMediator mediator) : ControllerBase
{
    [HttpGet("{petId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid petId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPetByIdQuery(petId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePetRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreatePetCommand(request.Name, request.Description, request.DateOfBirth, request.PetTypeValue, request.BreedValue),
            cancellationToken);

        return result.ToActionResult(petId => CreatedAtAction(nameof(GetById), new { petId }, null));
    }

    [HttpPut("{petId:guid}")]
    public async Task<IActionResult> Update(Guid petId, UpdatePetRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdatePetCommand(petId, request.Name, request.Description, request.DateOfBirth, request.PetTypeValue, request.BreedValue),
            cancellationToken);

        return result.ToNoContentResult();
    }

    [HttpDelete("{petId:guid}")]
    public async Task<IActionResult> Delete(Guid petId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeletePetCommand(petId), cancellationToken);
        return result.ToNoContentResult();
    }

    [HttpPost("{petId:guid}/images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(65 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 65 * 1024 * 1024)]
    public async Task<IActionResult> AddImages(Guid petId, IFormFileCollection files, CancellationToken cancellationToken)
    {
        var uploads = files
            .Select(f => new PetImageUpload(f.FileName, f.OpenReadStream(), f.ContentType, f.Length))
            .ToList();

        var result = await mediator.Send(new AddPetImagesCommand(petId, uploads), cancellationToken);

        if (result.Results.Any(r => !r.Success))
            return StatusCode(207, result);

        return StatusCode(201, result);
    }

    [HttpPost("{petId:guid}/images/batch-delete")]
    public async Task<IActionResult> BatchDeleteImages(
        Guid petId, BatchDeleteImagesRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new BatchDeletePetImagesCommand(petId, request.ImageIds), cancellationToken);
        return NoContent();
    }

    [HttpPut("{petId:guid}/images/{imageId:guid}/featured")]
    public async Task<IActionResult> SetFeaturedImage(Guid petId, Guid imageId, CancellationToken cancellationToken)
    {
        await mediator.Send(new SetFeaturedImageCommand(petId, imageId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{petId:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> RemoveImage(Guid petId, Guid imageId, CancellationToken cancellationToken)
    {
        await mediator.Send(new RemovePetImageCommand(petId, imageId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{petId:guid}/likes")]
    [AllowAnonymous]
    public async Task<IActionResult> IncrementLikes(Guid petId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new IncrementPetLikesCommand(petId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("{petId:guid}/likes")]
    [AllowAnonymous]
    public async Task<IActionResult> DecrementLikes(Guid petId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DecrementPetLikesCommand(petId), cancellationToken);
        return result.ToActionResult();
    }
}

public record CreatePetRequest(string Name, string? Description, DateOnly? DateOfBirth, int PetTypeValue, int BreedValue);
public record UpdatePetRequest(string Name, string? Description, DateOnly? DateOfBirth, int PetTypeValue, int BreedValue);
public record BatchDeleteImagesRequest(IReadOnlyList<Guid> ImageIds);
