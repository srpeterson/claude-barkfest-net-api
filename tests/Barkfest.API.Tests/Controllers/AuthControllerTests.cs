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
            username = $"alice{Guid.NewGuid():N}",
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
            username = $"firstuser{Guid.NewGuid():N}",
            firstName = "First",
            lastName = "User",
            email,
            phoneNumber = (string?)null,
            password = "SecurePass1!"
        });

        var response = await _client.PostAsJsonAsync("/v1/auth/register", new
        {
            username = $"seconduser{Guid.NewGuid():N}",
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
            username = $"noname{Guid.NewGuid():N}",
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
    public async Task Login_When_CredentialsAreValid_Returns_Ok()
    {
        var username = $"bob{Guid.NewGuid():N}";
        const string password = "SecurePass1!";

        await _client.PostAsJsonAsync("/v1/auth/register", new
        {
            username,
            firstName = "Bob",
            lastName = "Baker",
            email = $"bob-{Guid.NewGuid():N}@example.com",
            phoneNumber = (string?)null,
            password
        });

        var response = await _client.PostAsJsonAsync("/v1/auth/login", new { username, password });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Token must be in the response body, not a cookie
        response.Headers.TryGetValues("Set-Cookie", out _).ShouldBeFalse();

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("accountId", out _).ShouldBeTrue();
        doc.RootElement.TryGetProperty("accessToken", out var token).ShouldBeTrue();
        token.GetString().ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_When_PasswordIsWrong_Returns_NotFound()
    {
        var username = $"carol{Guid.NewGuid():N}";

        await _client.PostAsJsonAsync("/v1/auth/register", new
        {
            username,
            firstName = "Carol",
            lastName = "Clark",
            email = $"carol-{Guid.NewGuid():N}@example.com",
            phoneNumber = (string?)null,
            password = "CorrectPass1!"
        });

        var response = await _client.PostAsJsonAsync("/v1/auth/login", new
        {
            username,
            password = "WrongPass1!"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Login_When_UsernameNotFound_Returns_NotFound()
    {
        var response = await _client.PostAsJsonAsync("/v1/auth/login", new
        {
            username = $"nobody{Guid.NewGuid():N}",
            password = "AnyPass1!"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // -----------------------------------------------------------------------
    // POST /v1/auth/admin/login
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AdminLogin_When_CredentialsAreValid_Returns_Ok()
    {
        var adminClient = factory.CreateAuthenticatedAdminClient(Guid.NewGuid());
        var username = $"a{Guid.NewGuid():N}";
        const string password = "AdminPass1!";

        await adminClient.PostAsJsonAsync("/v1/admin/admins", new
        {
            username,
            name = "Login Test Admin",
            email = $"admin-login-{Guid.NewGuid():N}@barkfest.dev",
            phoneNumber = "+15555550100",
            password
        });

        var response = await _client.PostAsJsonAsync("/v1/auth/admin/login", new { username, password });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Token must be in the response body, not a cookie
        response.Headers.TryGetValues("Set-Cookie", out _).ShouldBeFalse();

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("accountId", out _).ShouldBeTrue();
        doc.RootElement.TryGetProperty("accessToken", out var token).ShouldBeTrue();
        token.GetString().ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task AdminLogin_When_UsernameNotFound_Returns_NotFound()
    {
        var response = await _client.PostAsJsonAsync("/v1/auth/admin/login", new
        {
            username = $"ghost{Guid.NewGuid():N}",
            password = "AnyPass1!"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AdminLogin_When_PasswordIsWrong_Returns_NotFound()
    {
        var adminClient = factory.CreateAuthenticatedAdminClient(Guid.NewGuid());
        var username = $"a{Guid.NewGuid():N}";

        await adminClient.PostAsJsonAsync("/v1/admin/admins", new
        {
            username,
            email = $"admin-wrongpw-{Guid.NewGuid():N}@barkfest.dev",
            password = "CorrectPass1!"
        });

        var response = await _client.PostAsJsonAsync("/v1/auth/admin/login", new
        {
            username,
            password = "WrongPass1!"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // -----------------------------------------------------------------------
    // GET /v1/auth/check-display-name
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CheckDisplayName_When_NameIsAvailable_Returns_Ok_WithAvailableTrue()
    {
        var response = await _client.GetAsync($"/v1/auth/check-display-name?value=UniqueName{Guid.NewGuid():N}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("available").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public async Task CheckDisplayName_When_NameIsTaken_Returns_Ok_WithAvailableFalse()
    {
        var displayName = $"Name{Guid.NewGuid().ToString("N")[..10]}"; // 14 chars — within 25-char limit
        await _client.PostAsJsonAsync("/v1/auth/register", new
        {
            username = $"user{Guid.NewGuid():N}",
            firstName = "Test",
            lastName = "User",
            email = $"taken-{Guid.NewGuid():N}@example.com",
            phoneNumber = (string?)null,
            password = "SecurePass1!",
            displayName
        });

        var response = await _client.GetAsync($"/v1/auth/check-display-name?value={Uri.EscapeDataString(displayName)}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("available").GetBoolean().ShouldBeFalse();
    }

    [Fact]
    public async Task CheckDisplayName_When_NameMatchesIgnoringCaseAndSpaces_Returns_Ok_WithAvailableFalse()
    {
        var suffix = Guid.NewGuid().ToString("N")[..6];
        var displayName = $"Cool {suffix}"; // 11 chars — within 25-char limit, contains a space
        await _client.PostAsJsonAsync("/v1/auth/register", new
        {
            username = $"user{Guid.NewGuid():N}",
            firstName = "Test",
            lastName = "User",
            email = $"spaces-{Guid.NewGuid():N}@example.com",
            phoneNumber = (string?)null,
            password = "SecurePass1!",
            displayName
        });

        // Strip spaces and uppercase — the endpoint normalizes to lowercase before comparing,
        // so sending uppercase verifies case-insensitivity end-to-end.
        var normalized = displayName.Replace(" ", "").ToUpper();
        var response = await _client.GetAsync($"/v1/auth/check-display-name?value={Uri.EscapeDataString(normalized)}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("available").GetBoolean().ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // POST /v1/auth/logout
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Logout_When_Called_Returns_NoContent()
    {
        var response = await _client.PostAsync("/v1/auth/logout", null);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

}
