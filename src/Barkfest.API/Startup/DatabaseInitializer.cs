using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Interfaces;
using Barkfest.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Barkfest.API.Startup;

public static class DatabaseInitializer
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        // Skip migration and seed in Testing — WebApplicationFactory runs migration via InitializeAsync.
        if (app.Environment.EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase))
            return;

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        await SeedAdminAsync(scope.ServiceProvider, app.Logger);
    }

    private static async Task SeedAdminAsync(IServiceProvider services, ILogger logger)
    {
        var settings = services.GetRequiredService<IOptions<AdminSeedSettings>>().Value;
        if (!settings.IsComplete)
            return;

        var adminUsername = settings.Username!;
        var adminName = settings.Name!;
        var adminEmail = settings.Email!;
        var adminPhoneNumber = settings.PhoneNumber!;
        var adminPassword = settings.Password!;

        var administratorRepository = services.GetRequiredService<IAdministratorRepository>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        var unitOfWork = services.GetRequiredService<IUnitOfWork>();

        var existing = await administratorRepository.GetByUsernameAsync(adminUsername, CancellationToken.None);
        if (existing is not null)
            return;

        var administrator = Barkfest.Domain.Entities.Administrator.Create(
            adminUsername, adminName, adminEmail, adminPhoneNumber, passwordHasher.Hash(adminPassword));

        await administratorRepository.AddAsync(administrator, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        logger.LogInformation("Administrator account seeded: username={Username}, email={Email}", adminUsername, adminEmail);
    }
}
