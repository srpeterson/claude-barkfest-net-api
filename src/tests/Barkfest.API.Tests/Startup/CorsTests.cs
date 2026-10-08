using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Barkfest.API.Tests.Startup;

public class CorsTests(BarkfestApiFactory factory) : IClassFixture<BarkfestApiFactory>
{
    private const string DefaultOrigin = "http://localhost:5173";
    private const string PublicEndpoint = "/v1/browse/pet-types";

    private static HttpRequestMessage GetFrom(string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, PublicEndpoint);
        request.Headers.Add("Origin", origin);
        return request;
    }

    private static HttpRequestMessage PreflightFrom(string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, PublicEndpoint);
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "authorization,content-type");
        return request;
    }

    [Fact]
    public async Task Request_When_OriginIsDefaultAllowedOrigin_Returns_AllowOriginAndCredentials()
    {
        var response = await factory.CreateClient().SendAsync(GetFrom(DefaultOrigin));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.GetValues("Access-Control-Allow-Origin").ShouldBe([DefaultOrigin]);
        response.Headers.GetValues("Access-Control-Allow-Credentials").ShouldBe(["true"]);
    }

    [Fact]
    public async Task Request_When_OriginIsNotAllowed_Returns_NoAllowOriginHeader()
    {
        var response = await factory.CreateClient().SendAsync(GetFrom("https://evil.example"));

        response.Headers.Contains("Access-Control-Allow-Origin").ShouldBeFalse();
    }

    [Fact]
    public async Task Preflight_When_OriginIsAllowed_Returns_AllowedMethodsAndHeaders()
    {
        var response = await factory.CreateClient().SendAsync(PreflightFrom(DefaultOrigin));

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.Headers.GetValues("Access-Control-Allow-Origin").ShouldBe([DefaultOrigin]);
        response.Headers.GetValues("Access-Control-Allow-Methods").ShouldNotBeEmpty();
        string.Join(",", response.Headers.GetValues("Access-Control-Allow-Headers"))
            .ShouldContain("authorization", Case.Insensitive);
    }

    [Fact]
    public async Task Preflight_When_OriginIsNotAllowed_Returns_NoAllowOriginHeader()
    {
        var response = await factory.CreateClient().SendAsync(PreflightFrom("https://evil.example"));

        response.Headers.Contains("Access-Control-Allow-Origin").ShouldBeFalse();
    }

    [Fact]
    public async Task Request_When_AllowedOriginIsConfigured_Returns_ConfiguredOriginOnly()
    {
        const string configured = "https://barkfest.example";
        using var configuredFactory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("Cors:AllowedOrigin", configured));
        var client = configuredFactory.CreateClient();

        var allowed = await client.SendAsync(GetFrom(configured));
        var defaultOrigin = await client.SendAsync(GetFrom(DefaultOrigin));

        allowed.Headers.GetValues("Access-Control-Allow-Origin").ShouldBe([configured]);
        defaultOrigin.Headers.Contains("Access-Control-Allow-Origin").ShouldBeFalse();
    }

    [Fact]
    public async Task Request_When_OriginIsAllowed_Exposes_LocationHeader()
    {
        var response = await factory.CreateClient().SendAsync(GetFrom(DefaultOrigin));

        string.Join(",", response.Headers.GetValues("Access-Control-Expose-Headers"))
            .ShouldContain("Location");
    }
}
