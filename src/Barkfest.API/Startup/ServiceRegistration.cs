using System.Text;
using Barkfest.API.Security;
using Barkfest.Application;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Infrastructure;
using Barkfest.Infrastructure.Security;
using Barkfest.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;

namespace Barkfest.API.Startup;

public static class ServiceRegistration
{
    public static WebApplicationBuilder AddBarkfestServices(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((ctx, lc) => lc
            .ReadFrom.Configuration(ctx.Configuration));

        builder.Services.AddControllers();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("BarkfestUI", policy =>
            {
                policy
                    .WithOrigins(
                        builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                var components = document.Components ??= new OpenApiComponents();
                components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter your JWT bearer token."
                });

                document.Info = new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Barkfest Web API",
                    Description = "Example API built with Claude Code",
                    Contact = new OpenApiContact
                    {
                        Name = "Cool Dudes, Inc",
                        Email = "srpeterson@outlook.com"
                    }
                };

                return Task.CompletedTask;
            });
        });

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

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var cookie = context.Request.Cookies["barkfest_auth"];
                        if (!string.IsNullOrEmpty(cookie))
                            context.Token = cookie;
                        return Task.CompletedTask;
                    }
                };

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

        return builder;
    }
}
