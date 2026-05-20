using Barkfest.API.Middleware;
using Scalar.AspNetCore;

namespace Barkfest.API.Startup;

public static class PipelineConfiguration
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.Authentication = new ScalarAuthenticationOptions
                {
                    PreferredSecuritySchemes = ["Bearer"]
                };
            });
        }

        app.UseHttpsRedirection();
        app.UseCors("BarkfestUI");
        app.UseAuthentication();
        app.UseMiddleware<ActiveOwnerMiddleware>();
        app.UseAuthorization();
        app.MapControllers();
        app.MapDefaultEndpoints();

        return app;
    }
}
