using Azure.Storage.Blobs;
using Barkfest.Persistence;
using Barkfest.Tests.Common.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http.Headers;
using Testcontainers.Azurite;
using Testcontainers.MsSql;

namespace Barkfest.API.Tests;

public class BarkfestApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    private readonly AzuriteContainer _azuriteContainer =
        new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:latest")
            .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace the real SQL Server DbContext with the test container connection.
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(_sqlContainer.GetConnectionString()));

            // Replace the Aspire-registered BlobServiceClient with a version-pinned one.
            // Azurite 3.35.0 does not yet support the 2025-05-05 service version that
            // Azure.Storage.Blobs 12.28.0 uses by default.
            services.RemoveAll<BlobServiceClient>();
            services.AddSingleton(_ =>
            {
                var options = new BlobClientOptions(BlobClientOptions.ServiceVersion.V2024_11_04);
                return new BlobServiceClient(_azuriteContainer.GetConnectionString(), options);
            });
        });
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_sqlContainer.StartAsync(), _azuriteContainer.StartAsync());

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public HttpClient CreateAuthenticatedClient(Guid ownerId)
    {
        var client = CreateClient();
        var token = JwtTestHelper.GenerateOwnerToken(ownerId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public HttpClient CreateAuthenticatedAdminClient(Guid adminId)
    {
        var client = CreateClient();
        var token = JwtTestHelper.GenerateAdminToken(adminId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public new async Task DisposeAsync()
    {
        await _sqlContainer.DisposeAsync();
        await _azuriteContainer.DisposeAsync();
    }
}
