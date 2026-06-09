using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Barkfest.Persistence.Tests;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public string ConnectionString => _container.GetConnectionString();

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new AppDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await WaitForSqlServerAsync();

        await using var ctx = CreateContext();
        await ctx.Database.MigrateAsync();
    }

    private async Task WaitForSqlServerAsync()
    {
        const int maxAttempts = 20;
        var delay = TimeSpan.FromSeconds(2);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await using var connection = new SqlConnection(ConnectionString);
                await connection.OpenAsync();
                return;
            }
            catch when (attempt < maxAttempts)
            {
                await Task.Delay(delay);
            }
        }

        throw new InvalidOperationException("SQL Server container did not become ready in time.");
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();
}
