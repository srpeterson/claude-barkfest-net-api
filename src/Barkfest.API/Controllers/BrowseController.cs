using Barkfest.Application.Features.Browse.Queries;
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
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBrowseImagesQuery(petType, breed), cancellationToken);
        return Ok(result);
    }
}
