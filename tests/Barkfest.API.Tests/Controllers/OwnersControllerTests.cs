using System.Net;
using System.Net.Http.Json;

namespace Barkfest.API.Tests.Controllers;

public class OwnersControllerTests(BarkfestApiFactory factory)
    : IClassFixture<BarkfestApiFactory>
{
    private readonly HttpClient _unauthenticatedClient = factory.CreateClient();

    // -----------------------------------------------------------------------
    // GET /v1/owners — get all owners (admin only)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAll_When_Unauthenticated_Returns_Unauthorized()
    {
        var response = await _unauthenticatedClient.GetAsync("/v1/owners");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_When_CallerIsNotAdmin_Returns_Forbidden()
    {
        var (client, _) = await RegisterAndGetClient();

        var response = await client.GetAsync("/v1/owners");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAll_When_CallerIsAdmin_Returns_Ok()
    {
        var adminClient = factory.CreateAuthenticatedAdminClient(Guid.NewGuid());

        var response = await adminClient.GetAsync("/v1/owners");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // -----------------------------------------------------------------------
    // GET /v1/owners/{id} — auth
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetById_When_Unauthenticated_Returns_Unauthorized()
    {
        var response = await _unauthenticatedClient.GetAsync($"/v1/owners/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_When_OwnerExists_Returns_Owner()
    {
        var (client, id) = await RegisterAndGetClient();

        var response = await client.GetAsync($"/v1/owners/{id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("Alice");
    }

    [Fact]
    public async Task GetById_When_OwnerNotFound_Returns_NotFound()
    {
        var (client, _) = await RegisterAndGetClient();

        var response = await client.GetAsync($"/v1/owners/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_When_NotOwner_Returns_Ok()
    {
        var (_, id) = await RegisterAndGetClient("alice-other");

        // A different authenticated owner can view any owner's profile
        var (otherClient, _) = await RegisterAndGetClient("other-owner");

        var response = await otherClient.GetAsync($"/v1/owners/{id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // -----------------------------------------------------------------------
    // PUT /v1/owners/{id}
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Update_When_OwnerExists_Returns_NoContent()
    {
        var (client, id) = await RegisterAndGetClient("bob");

        var response = await client.PutAsJsonAsync($"/v1/owners/{id}", new
        {
            firstName = "Robert",
            lastName = "Baker",
            email = $"robert-{Guid.NewGuid():N}@example.com",
            phoneNumber = (string?)null
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_When_OwnerNotFound_Returns_NotFound()
    {
        var (client, _) = await RegisterAndGetClient("ghost-owner");

        var response = await client.PutAsJsonAsync($"/v1/owners/{Guid.NewGuid()}", new
        {
            firstName = "Ghost",
            lastName = "User",
            email = "ghost@example.com",
            phoneNumber = (string?)null
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_When_Unauthenticated_Returns_Unauthorized()
    {
        var response = await _unauthenticatedClient.PutAsJsonAsync($"/v1/owners/{Guid.NewGuid()}", new
        {
            firstName = "Ghost",
            lastName = "User",
            email = "ghost@example.com",
            phoneNumber = (string?)null
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    // -----------------------------------------------------------------------
    // DELETE /v1/owners/{id}
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Delete_When_OwnerExists_Returns_NoContent()
    {
        var (client, id) = await RegisterAndGetClient("carol");

        var response = await client.DeleteAsync($"/v1/owners/{id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_When_OwnerNotFound_Returns_NotFound()
    {
        var (client, _) = await RegisterAndGetClient("del-ghost");

        var response = await client.DeleteAsync($"/v1/owners/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // -----------------------------------------------------------------------
    // GET /v1/owners/{id}/pets
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetPets_When_OwnerExists_Returns_Ok()
    {
        var (client, id) = await RegisterAndGetClient("dave");

        var response = await client.GetAsync($"/v1/owners/{id}/pets");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private async Task<(HttpClient client, Guid ownerId)> RegisterAndGetClient(string seed = "alice")
    {
        var username = $"{seed}{Guid.NewGuid():N}";

        var registerResponse = await _unauthenticatedClient.PostAsJsonAsync("/v1/auth/register", new
        {
            username,
            firstName = "Alice",
            lastName = "Adams",
            email = $"{seed}-{Guid.NewGuid():N}@example.com",
            phoneNumber = (string?)null,
            password = "SecurePass1!"
        });
        registerResponse.EnsureSuccessStatusCode();

        var location = registerResponse.Headers.Location!.ToString();
        var ownerId = Guid.Parse(location.Split('/').Last());

        return (factory.CreateAuthenticatedClient(ownerId), ownerId);
    }
}
