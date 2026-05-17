using Barkfest.API.Middleware;
using Barkfest.Application;
using Barkfest.Infrastructure;
using Barkfest.Persistence;
using Microsoft.EntityFrameworkCore;
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

var app = builder.Build();

// Skip migration in Testing — WebApplicationFactory runs it via InitializeAsync.
if (!app.Environment.EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider
               .GetRequiredService<AppDbContext>()
               .Database.MigrateAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapDefaultEndpoints();

await app.RunAsync();

public partial class Program;
