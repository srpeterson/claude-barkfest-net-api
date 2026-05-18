using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Barkfest.API.Tests.Controllers;

public class OwnersControllerTests(BarkfestApiFactory factory)
    : IClassFixture<BarkfestApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions =
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    // -----------------------------------------------------------------------
    // GET /v1/owners
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAll_When_Called_Returns_Ok()
    {
        var response = await _client.GetAsync("/v1/owners");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // -----------------------------------------------------------------------
    // POST /v1/owners
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Create_When_RequestIsValid_Returns_Created()
    {
        var response = await _client.PostAsJsonAsync("/v1/owners", new
        {
            firstName = "John",
            lastName = "Doe",
            email = "john.doe@example.com",
            phoneNumber = (string?)null
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location!.ToString().ShouldContain("/v1/owners/");
    }

    [Fact]
    public async Task Create_When_EmailIsMissing_Returns_BadRequest()
    {
        var response = await _client.PostAsJsonAsync("/v1/owners", new
        {
            firstName = "Jane",
            lastName = "Smith",
            email = ""
        });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("Validation failed");
    }

    // -----------------------------------------------------------------------
    // GET /v1/owners/{id}
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetById_When_OwnerExists_Returns_Owner()
    {
        var id = await CreateOwner("Alice", "Adams", "alice@example.com");

        var response = await _client.GetAsync($"/v1/owners/{id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("Alice");
        body.ShouldContain("adams@example.com".Replace("adams", "alice"));
    }

    [Fact]
    public async Task GetById_When_OwnerNotFound_Returns_NotFound()
    {
        var response = await _client.GetAsync($"/v1/owners/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("Not Found");
    }

    // -----------------------------------------------------------------------
    // PUT /v1/owners/{id}
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Update_When_OwnerExists_Returns_NoContent()
    {
        var id = await CreateOwner("Bob", "Baker", "bob@example.com");

        var response = await _client.PutAsJsonAsync($"/v1/owners/{id}", new
        {
            firstName = "Robert",
            lastName = "Baker",
            email = "robert@example.com",
            phoneNumber = (string?)null
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_When_OwnerNotFound_Returns_NotFound()
    {
        var response = await _client.PutAsJsonAsync($"/v1/owners/{Guid.NewGuid()}", new
        {
            firstName = "Ghost",
            lastName = "User",
            email = "ghost@example.com",
            phoneNumber = (string?)null
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // -----------------------------------------------------------------------
    // DELETE /v1/owners/{id}
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Delete_When_OwnerExists_Returns_NoContent()
    {
        var id = await CreateOwner("Carol", "Clark", "carol@example.com");

        var response = await _client.DeleteAsync($"/v1/owners/{id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_When_OwnerNotFound_Returns_NotFound()
    {
        var response = await _client.DeleteAsync($"/v1/owners/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // -----------------------------------------------------------------------
    // GET /v1/owners/{id}/pets
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetPets_When_OwnerExists_Returns_Ok()
    {
        var id = await CreateOwner("Dave", "Davis", "dave@example.com");

        var response = await _client.GetAsync($"/v1/owners/{id}/pets");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private async Task<Guid> CreateOwner(string firstName, string lastName, string email)
    {
        var response = await _client.PostAsJsonAsync("/v1/owners", new
        {
            firstName,
            lastName,
            email,
            phoneNumber = (string?)null
        });

        response.EnsureSuccessStatusCode();

        // Extract the id from the Location header: /v1/owners/{id}
        var location = response.Headers.Location!.ToString();
        return Guid.Parse(location.Split('/').Last());
    }
}
