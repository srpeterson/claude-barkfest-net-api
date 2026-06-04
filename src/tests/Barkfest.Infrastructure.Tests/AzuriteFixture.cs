using Azure.Storage.Blobs;
using Testcontainers.Azurite;

namespace Barkfest.Infrastructure.Tests;

public class AzuriteFixture : IAsyncLifetime
{
    private readonly AzuriteContainer _container = new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:latest")
        .Build();

    public BlobServiceClient CreateBlobServiceClient()
    {
        // Pin to a service version that Azurite supports. Azure.Storage.Blobs 12.28.0
        // defaults to service version 2025-05-05 which the current Azurite image does
        // not yet recognise, causing InvalidHeaderValue errors on every request.
        var options = new BlobClientOptions(BlobClientOptions.ServiceVersion.V2024_11_04);
        return new BlobServiceClient(_container.GetConnectionString(), options);
    }

    public async Task InitializeAsync() => await _container.StartAsync();

    public async Task DisposeAsync() => await _container.DisposeAsync();
}
