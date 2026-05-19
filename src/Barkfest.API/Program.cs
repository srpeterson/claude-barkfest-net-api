using System.Text;
using Barkfest.API.Middleware;
using Barkfest.API.Security;
using Barkfest.Application;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Interfaces;
using Barkfest.Infrastructure;
using Barkfest.Infrastructure.Security;
using Barkfest.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.AddInfrastructure();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Prevent claim names from being remapped to WS-Federation URIs so that
        // "sub" stays "sub" (not ClaimTypes.NameIdentifier) for CurrentUserService.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

var app = builder.Build();

// Skip migration and seed in Testing — WebApplicationFactory runs migration via InitializeAsync.
if (!app.Environment.EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    await SeedAdminAsync(scope.ServiceProvider, app.Configuration, app.Logger);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<ActiveOwnerMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultEndpoints();

await app.RunAsync();

static async Task SeedAdminAsync(IServiceProvider services, IConfiguration configuration, Microsoft.Extensions.Logging.ILogger logger)
{
    var adminUsername = configuration["Admin:Username"];
    var adminName = configuration["Admin:Name"];
    var adminEmail = configuration["Admin:Email"];
    var adminPhoneNumber = configuration["Admin:PhoneNumber"];
    var adminPassword = configuration["Admin:Password"];

    if (string.IsNullOrWhiteSpace(adminUsername) || string.IsNullOrWhiteSpace(adminName) ||
        string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPhoneNumber) ||
        string.IsNullOrWhiteSpace(adminPassword))
        return;

    var administratorRepository = services.GetRequiredService<IAdministratorRepository>();
    var passwordHasher = services.GetRequiredService<IPasswordHasher>();
    var unitOfWork = services.GetRequiredService<IUnitOfWork>();

    var existing = await administratorRepository.GetByUsernameAsync(adminUsername, CancellationToken.None);
    if (existing is not null)
        return;

    var administrator = new Barkfest.Domain.Entities.Administrator();
    administrator.SetUsername(adminUsername);
    administrator.SetName(adminName);
    administrator.SetEmail(adminEmail);
    administrator.SetPhoneNumber(adminPhoneNumber);
    administrator.SetPasswordHash(passwordHasher.Hash(adminPassword));

    await administratorRepository.AddAsync(administrator, CancellationToken.None);
    await unitOfWork.SaveChangesAsync(CancellationToken.None);

    logger.LogInformation("Administrator account seeded: username={Username}, email={Email}", adminUsername, adminEmail);
}

public partial class Program;
