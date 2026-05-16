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
    // GET /api/owners
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/owners");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // -----------------------------------------------------------------------
    // POST /api/owners
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Create_ValidRequest_Returns201WithLocationHeader()
    {
        var response = await _client.PostAsJsonAsync("/api/owners", new
        {
            firstName = "John",
            lastName = "Doe",
            email = "john.doe@example.com",
            phoneNumber = (string?)null
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location!.ToString().ShouldContain("/api/owners/");
    }

    [Fact]
    public async Task Create_MissingEmail_Returns400ValidationProblem()
    {
        var response = await _client.PostAsJsonAsync("/api/owners", new
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
    // GET /api/owners/{id}
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetById_ExistingOwner_ReturnsOwnerData()
    {
        var id = await CreateOwner("Alice", "Adams", "alice@example.com");

        var response = await _client.GetAsync($"/api/owners/{id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("Alice");
        body.ShouldContain("adams@example.com".Replace("adams", "alice"));
    }

    [Fact]
    public async Task GetById_UnknownId_Returns404ProblemDetails()
    {
        var response = await _client.GetAsync($"/api/owners/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("Not Found");
    }

    // -----------------------------------------------------------------------
    // PUT /api/owners/{id}
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Update_ExistingOwner_Returns204()
    {
        var id = await CreateOwner("Bob", "Baker", "bob@example.com");

        var response = await _client.PutAsJsonAsync($"/api/owners/{id}", new
        {
            firstName = "Robert",
            lastName = "Baker",
            email = "robert@example.com",
            phoneNumber = (string?)null
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_UnknownId_Returns404()
    {
        var response = await _client.PutAsJsonAsync($"/api/owners/{Guid.NewGuid()}", new
        {
            firstName = "Ghost",
            lastName = "User",
            email = "ghost@example.com",
            phoneNumber = (string?)null
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // -----------------------------------------------------------------------
    // DELETE /api/owners/{id}
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Delete_ExistingOwner_Returns204()
    {
        var id = await CreateOwner("Carol", "Clark", "carol@example.com");

        var response = await _client.DeleteAsync($"/api/owners/{id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_UnknownId_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/owners/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // -----------------------------------------------------------------------
    // GET /api/owners/{id}/pets
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetPets_ExistingOwner_ReturnsOk()
    {
        var id = await CreateOwner("Dave", "Davis", "dave@example.com");

        var response = await _client.GetAsync($"/api/owners/{id}/pets");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private async Task<Guid> CreateOwner(string firstName, string lastName, string email)
    {
        var response = await _client.PostAsJsonAsync("/api/owners", new
        {
            firstName,
            lastName,
            email,
            phoneNumber = (string?)null
        });

        response.EnsureSuccessStatusCode();

        // Extract the id from the Location header: /api/owners/{id}
        var location = response.Headers.Location!.ToString();
        return Guid.Parse(location.Split('/').Last());
    }
}
