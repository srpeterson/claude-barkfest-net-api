using Barkfest.Application.Features.Browse.Queries;
using Barkfest.Application.Features.Browse.Queries.GetBrowseBreeds;
using Barkfest.Application.Features.Browse.Queries.GetBrowsePetTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Barkfest.API.Controllers;

[ApiController]
[Route("v1/browse")]
[AllowAnonymous]
public class BrowseController(IMediator mediator) : ControllerBase
{
    [HttpGet("images")]
    public async Task<IActionResult> GetImages(
        [FromQuery] string? petType,
        [FromQuery] string? breed,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 6,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetBrowseImagesQuery(petType, breed, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("pet-types")]
    public async Task<IActionResult> GetPetTypes(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBrowsePetTypesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("breeds")]
    public async Task<IActionResult> GetBreeds(
        [FromQuery] string petType,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBrowseBreedsQuery(petType), cancellationToken);
        return Ok(result);
    }
}
