using Barkfest.Application.Common.Interfaces;
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

        builder.Services.Configure<JwtSettings>(
            builder.Configuration.GetSection("Jwt"));

        return builder;
    }
}
