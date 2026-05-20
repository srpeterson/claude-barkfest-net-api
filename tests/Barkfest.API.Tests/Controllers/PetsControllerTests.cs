using System.Net;
using System.Net.Http.Json;

namespace Barkfest.API.Tests.Controllers;

public class PetsControllerTests(BarkfestApiFactory factory)
    : IClassFixture<BarkfestApiFactory>
{
    private readonly HttpClient _unauthenticatedClient = factory.CreateClient();

    // -----------------------------------------------------------------------
    // POST /v1/pets — auth
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Create_When_Unauthenticated_Returns_Unauthorized()
    {
        var response = await _unauthenticatedClient.PostAsJsonAsync("/v1/pets", new
        {
            name = "Buddy",
            description = (string?)null,
            dateOfBirth = (string?)null,
            petType = "Dog"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_When_RequestIsValid_Returns_Created()
    {
        var (client, _) = await RegisterAndGetClient();

        var response = await client.PostAsJsonAsync("/v1/pets", new
        {
            name = "Buddy",
            description = (string?)null,
            dateOfBirth = (string?)null,
            petType = "Dog"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location!.ToString().ShouldContain("/v1/pets/");
    }

    [Fact]
    public async Task Create_When_NameIsMissing_Returns_BadRequest()
    {
        var (client, _) = await RegisterAndGetClient();

        var response = await client.PostAsJsonAsync("/v1/pets", new
        {
            name = "",
            description = (string?)null,
            dateOfBirth = (string?)null,
            petType = "Dog"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("Validation failed");
    }

    // -----------------------------------------------------------------------
    // GET /v1/pets/{id}
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetById_When_Unauthenticated_Returns_Unauthorized()
    {
        var response = await _unauthenticatedClient.GetAsync($"/v1/pets/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_When_PetExists_Returns_Pet()
    {
        var (client, ownerId) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Luna");

        var response = await client.GetAsync($"/v1/pets/{petId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("Luna");
    }

    [Fact]
    public async Task GetById_When_PetNotFound_Returns_NotFound()
    {
        var (client, _) = await RegisterAndGetClient();

        var response = await client.GetAsync($"/v1/pets/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_When_NotOwner_Returns_Ok()
    {
        var (ownerClient, _) = await RegisterAndGetClient();
        var petId = await CreatePet(ownerClient, "Buddy");

        // A different authenticated owner can view any pet
        var (otherClient, _) = await RegisterAndGetClient();

        var response = await otherClient.GetAsync($"/v1/pets/{petId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // -----------------------------------------------------------------------
    // PUT /v1/pets/{id}
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Update_When_PetExists_Returns_NoContent()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Max");

        var response = await client.PutAsJsonAsync($"/v1/pets/{petId}", new
        {
            name = "Maxwell",
            description = "Very good boy",
            dateOfBirth = (string?)null,
            petType = "Dog"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_When_PetNotFound_Returns_NotFound()
    {
        var (client, _) = await RegisterAndGetClient();

        var response = await client.PutAsJsonAsync($"/v1/pets/{Guid.NewGuid()}", new
        {
            name = "Ghost",
            description = (string?)null,
            dateOfBirth = (string?)null,
            petType = "Dog"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // -----------------------------------------------------------------------
    // DELETE /v1/pets/{id}
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Delete_When_PetExists_Returns_NoContent()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Rex");

        var response = await client.DeleteAsync($"/v1/pets/{petId}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_When_PetNotFound_Returns_NotFound()
    {
        var (client, _) = await RegisterAndGetClient();

        var response = await client.DeleteAsync($"/v1/pets/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private async Task<(HttpClient client, Guid ownerId)> RegisterAndGetClient()
    {
        var registerResponse = await _unauthenticatedClient.PostAsJsonAsync("/v1/auth/register", new
        {
            username = $"petowner{Guid.NewGuid():N}",
            firstName = "Test",
            lastName = "Owner",
            email = $"petowner-{Guid.NewGuid():N}@example.com",
            phoneNumber = (string?)null,
            password = "SecurePass1!"
        });
        registerResponse.EnsureSuccessStatusCode();

        var location = registerResponse.Headers.Location!.ToString();
        var ownerId = Guid.Parse(location.Split('/').Last());

        return (factory.CreateAuthenticatedClient(ownerId), ownerId);
    }

    private async Task<Guid> CreatePet(HttpClient client, string name, string petType = "Dog")
    {
        var response = await client.PostAsJsonAsync("/v1/pets", new
        {
            name,
            description = (string?)null,
            dateOfBirth = (string?)null,
            petType
        });

        response.EnsureSuccessStatusCode();
        var location = response.Headers.Location!.ToString();
        return Guid.Parse(location.Split('/').Last());
    }
}
