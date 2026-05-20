using Barkfest.Application.Common.Interfaces;
using Barkfest.Infrastructure.Moderation;
using Barkfest.Infrastructure.Security;
using Barkfest.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Barkfest.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddAzureBlobServiceClient("barkfest-blobs");

        builder.Services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
        builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        // TODO: Replace with AzureContentModerationService after Azure AI Content Safety is provisioned.
        builder.Services.AddSingleton<IContentModerationService, NoOpContentModerationService>();

        builder.Services.Configure<JwtSettings>(
            builder.Configuration.GetSection("Jwt"));

        return builder;
    }
}
