namespace Barkfest.Application.Common;

/// <summary>
/// Canonical blob-storage container names. These are the single source of truth for
/// every upload/delete handler and for the public <c>ImagesController</c> whitelist -
/// never hardcode the string literals at a call site.
/// </summary>
public static class BlobContainers
{
    public const string PetImages = "pet-images";
    public const string OwnerProfileImages = "owner-profile-images";

    /// <summary>
    /// The containers the anonymous image endpoint is permitted to serve. Adding a new
    /// container here makes it publicly readable - do so deliberately. Private containers
    /// (e.g. future moderation artifacts) must NOT be listed.
    /// </summary>
    public static bool IsPubliclyServable(string containerName) =>
        containerName is PetImages or OwnerProfileImages;
}
