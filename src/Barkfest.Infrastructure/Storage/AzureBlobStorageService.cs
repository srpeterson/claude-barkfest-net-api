using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Barkfest.Application.Common.Interfaces;

namespace Barkfest.Infrastructure.Storage;

public class AzureBlobStorageService(BlobServiceClient blobServiceClient) : IBlobStorageService
{
    public async Task UploadAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var container = blobServiceClient.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blob = container.GetBlobClient(blobName);
        await blob.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, cancellationToken);
    }

    public async Task<Stream> DownloadAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var blob = blobServiceClient
            .GetBlobContainerClient(containerName)
            .GetBlobClient(blobName);

        var response = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Value.Content;
    }

    public async Task DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var blob = blobServiceClient
            .GetBlobContainerClient(containerName)
            .GetBlobClient(blobName);

        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var blob = blobServiceClient
            .GetBlobContainerClient(containerName)
            .GetBlobClient(blobName);

        return await blob.ExistsAsync(cancellationToken);
    }
}
