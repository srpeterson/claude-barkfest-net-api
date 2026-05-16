using Azure.Storage.Blobs;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Barkfest.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureBlobStorage")
            ?? throw new InvalidOperationException("Connection string 'AzureBlobStorage' is missing.");

        services.AddSingleton(_ => new BlobServiceClient(connectionString));
        services.AddScoped<IBlobStorageService, AzureBlobStorageService>();

        return services;
    }
}
