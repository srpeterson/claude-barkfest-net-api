namespace Barkfest.Domain.ValueObjects;

public static class SupportedImageType
{
    public static readonly IReadOnlyCollection<string> AllowedContentTypes =
        ["image/jpeg", "image/jpg", "image/png"];

    public static readonly IReadOnlyCollection<string> AllowedExtensions =
        [".jpeg", ".jpg", ".png"];

    public static bool IsAllowedContentType(string contentType) =>
        AllowedContentTypes.Contains(contentType.Trim().ToLowerInvariant());

    public static bool IsAllowedExtension(string fileName) =>
        AllowedExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant());
}
