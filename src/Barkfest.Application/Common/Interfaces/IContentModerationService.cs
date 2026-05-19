namespace Barkfest.Application.Common.Interfaces;

public interface IContentModerationService
{
    Task<bool> IsImageSafeAsync(Stream imageStream, CancellationToken cancellationToken = default);
}
