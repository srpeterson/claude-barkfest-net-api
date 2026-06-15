using Barkfest.Application.Common;
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
        // Anonymous endpoint: serve only the whitelisted public containers, never an
        // arbitrary container name from the URL. 404 (not 403) avoids confirming which
        // container names exist.
        if (!BlobContainers.IsPubliclyServable(containerName))
            return NotFound();

        var exists = await blobStorageService.ExistsAsync(containerName, blobName, cancellationToken);
        if (!exists)
            return NotFound();

        var stream = await blobStorageService.DownloadAsync(containerName, blobName, cancellationToken);
        Response.Headers.CacheControl = CacheControlFor(containerName);
        return File(stream, GetContentType(blobName));
    }

    // Blob names embed a GUID and are never reused (replacement creates a new blob, the old
    // URL is abandoned), so the bytes at any URL are immutable - cache aggressively.
    //   - Pet images are genuinely public: cacheable by browsers, proxies, and CDNs.
    //   - Profile images are unlisted (no auth check, protected only by an unguessable URL).
    //     Use "private" so only the end user's browser caches them, never shared caches/CDNs -
    //     this keeps the door open for real access control later. See ROADMAP.
    private static string CacheControlFor(string containerName) =>
        containerName == BlobContainers.PetImages
            ? "public, max-age=31536000, immutable"
            : "private, max-age=31536000, immutable";

    private static string GetContentType(string blobName) =>
        Path.GetExtension(blobName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png"            => "image/png",
            _                 => "application/octet-stream"
        };
}
