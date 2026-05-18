using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Barkfest.API.Tests.Controllers;

public class AuthControllerTests(BarkfestApiFactory factory)
    : IClassFixture<BarkfestApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    // -----------------------------------------------------------------------
    // POST /v1/auth/register
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Register_When_ValidRequest_Returns_Created()
    {
        var response = await _client.PostAsJsonAsync("/v1/auth/register", new
        {
            firstName = "Alice",
            lastName = "Adams",
            email = $"alice-{Guid.NewGuid():N}@example.com",
            phoneNumber = (string?)null,
            password = "SecurePass1!"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location!.ToString().ShouldContain("/v1/owners/");
    }

    [Fact]
    public async Task Register_When_EmailAlreadyInUse_Returns_BadRequest()
    {
        var email = $"dup-{Guid.NewGuid():N}@example.com";

        await _client.PostAsJsonAsync("/v1/auth/register", new
        {
            firstName = "First",
            lastName = "User",
            email,
            phoneNumber = (string?)null,
            password = "SecurePass1!"
        });

        var response = await _client.PostAsJsonAsync("/v1/auth/register", new
        {
            firstName = "Second",
            lastName = "User",
            email,
            phoneNumber = (string?)null,
            password = "SecurePass1!"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_When_NameMissing_Returns_BadRequest()
    {
        var response = await _client.PostAsJsonAsync("/v1/auth/register", new
        {
            firstName = "",
            lastName = "Adams",
            email = $"noname-{Guid.NewGuid():N}@example.com",
            phoneNumber = (string?)null,
            password = "SecurePass1!"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("Validation failed");
    }

    // -----------------------------------------------------------------------
    // POST /v1/auth/login
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Login_When_CredentialsAreValid_Returns_Ok_WithToken()
    {
        var email = $"login-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass1!";

        await _client.PostAsJsonAsync("/v1/auth/register", new
        {
            firstName = "Bob",
            lastName = "Baker",
            email,
            phoneNumber = (string?)null,
            password
        });

        var response = await _client.PostAsJsonAsync("/v1/auth/login", new
        {
            email,
            password
        });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("accessToken", out _).ShouldBeTrue();
        doc.RootElement.TryGetProperty("accountId", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task Login_When_PasswordIsWrong_Returns_NotFound()
    {
        var email = $"wrongpw-{Guid.NewGuid():N}@example.com";

        await _client.PostAsJsonAsync("/v1/auth/register", new
        {
            firstName = "Carol",
            lastName = "Clark",
            email,
            phoneNumber = (string?)null,
            password = "CorrectPass1!"
        });

        var response = await _client.PostAsJsonAsync("/v1/auth/login", new
        {
            email,
            password = "WrongPass1!"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Login_When_EmailNotFound_Returns_NotFound()
    {
        var response = await _client.PostAsJsonAsync("/v1/auth/login", new
        {
            email = $"nobody-{Guid.NewGuid():N}@example.com",
            password = "AnyPass1!"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // -----------------------------------------------------------------------
    // POST /v1/auth/admin/login
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AdminLogin_When_CredentialsAreValid_Returns_Ok_WithToken()
    {
        // Create an admin via the admin endpoint first
        var adminClient = factory.CreateAuthenticatedAdminClient(Guid.NewGuid());
        var email = $"admin-login-{Guid.NewGuid():N}@barkfest.dev";
        const string password = "AdminPass1!";

        await adminClient.PostAsJsonAsync("/v1/admin/admins", new { email, password });

        var response = await _client.PostAsJsonAsync("/v1/auth/admin/login", new { email, password });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("accessToken", out _).ShouldBeTrue();
        doc.RootElement.TryGetProperty("accountId", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task AdminLogin_When_EmailNotFound_Returns_NotFound()
    {
        var response = await _client.PostAsJsonAsync("/v1/auth/admin/login", new
        {
            email = $"nobody-{Guid.NewGuid():N}@barkfest.dev",
            password = "AnyPass1!"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AdminLogin_When_PasswordIsWrong_Returns_NotFound()
    {
        var adminClient = factory.CreateAuthenticatedAdminClient(Guid.NewGuid());
        var email = $"admin-wrongpw-{Guid.NewGuid():N}@barkfest.dev";

        await adminClient.PostAsJsonAsync("/v1/admin/admins", new { email, password = "CorrectPass1!" });

        var response = await _client.PostAsJsonAsync("/v1/auth/admin/login", new
        {
            email,
            password = "WrongPass1!"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
