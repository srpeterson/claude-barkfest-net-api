using Barkfest.Application.Common.Interfaces;

namespace Barkfest.Infrastructure.Moderation;

// TODO: Replace with AzureContentModerationService after deploying to Azure.
// Use Azure AI Content Safety (successor to deprecated Azure Content Moderator).
// Docs: https://learn.microsoft.com/en-us/azure/ai-services/content-safety/
// Steps:
//   1. Provision an Azure AI Content Safety resource in your Azure subscription
//   2. Add the connection string/key to appsettings (injected via Aspire or Key Vault)
//   3. Install NuGet: Azure.AI.ContentSafety
//   4. Implement AzureContentModerationService : IContentModerationService
//      - Call ImageClient.AnalyzeImageAsync(imageStream)
//      - Reject if any category (Hate, Sexual, Violence, SelfHarm) exceeds threshold
//   5. Swap registration in DependencyInjection.cs
public class NoOpContentModerationService : IContentModerationService
{
    public Task<bool> IsImageSafeAsync(Stream imageStream, CancellationToken cancellationToken = default)
        => Task.FromResult(true);
}
