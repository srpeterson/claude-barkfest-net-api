using Barkfest.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Barkfest.API.Controllers;

[ApiController]
[Route("v1/images")]
[AllowAnonymous]
public class ImagesController(IBlobStorageService blobStorageService) : ControllerBase
{
    [HttpGet("{containerName}/{*blobName}")]
    public async Task<IActionResult> GetImage(
        string containerName,
        string blobName,
        CancellationToken cancellationToken)
    {
        var exists = await blobStorageService.ExistsAsync(containerName, blobName, cancellationToken);
        if (!exists)
            return NotFound();

        var stream = await blobStorageService.DownloadAsync(containerName, blobName, cancellationToken);
        Response.Headers.CacheControl = "no-store";
        return File(stream, GetContentType(blobName));
    }

    private static string GetContentType(string blobName) =>
        Path.GetExtension(blobName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png"            => "image/png",
            _                 => "application/octet-stream"
        };
}
