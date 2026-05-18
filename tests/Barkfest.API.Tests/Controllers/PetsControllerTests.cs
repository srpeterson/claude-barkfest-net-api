using System.Net;
using System.Net.Http.Json;

namespace Barkfest.API.Tests.Controllers;

public class PetsControllerTests(BarkfestApiFactory factory)
    : IClassFixture<BarkfestApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    // -----------------------------------------------------------------------
    // GET /v1/pets
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAll_When_Called_Returns_Ok()
    {
        var response = await _client.GetAsync("/v1/pets");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // -----------------------------------------------------------------------
    // POST /v1/pets
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Create_When_RequestIsValid_Returns_Created()
    {
        var ownerId = await CreateOwner();

        var response = await _client.PostAsJsonAsync("/v1/pets", new
        {
            ownerId,
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
    public async Task Create_When_OwnerNotFound_Returns_NotFound()
    {
        var response = await _client.PostAsJsonAsync("/v1/pets", new
        {
            ownerId = Guid.NewGuid(),
            name = "Ghost",
            description = (string?)null,
            dateOfBirth = (string?)null,
            petType = "Dog"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_When_NameIsMissing_Returns_BadRequest()
    {
        var ownerId = await CreateOwner();

        var response = await _client.PostAsJsonAsync("/v1/pets", new
        {
            ownerId,
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
    public async Task GetById_When_PetExists_Returns_Pet()
    {
        var ownerId = await CreateOwner();
        var petId = await CreatePet(ownerId, "Luna", "Cat");

        var response = await _client.GetAsync($"/v1/pets/{petId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("Luna");
    }

    [Fact]
    public async Task GetById_When_PetNotFound_Returns_NotFound()
    {
        var response = await _client.GetAsync($"/v1/pets/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("Not Found");
    }

    // -----------------------------------------------------------------------
    // PUT /v1/pets/{id}
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Update_When_PetExists_Returns_NoContent()
    {
        var ownerId = await CreateOwner();
        var petId = await CreatePet(ownerId, "Max", "Dog");

        var response = await _client.PutAsJsonAsync($"/v1/pets/{petId}", new
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
        var response = await _client.PutAsJsonAsync($"/v1/pets/{Guid.NewGuid()}", new
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
        var ownerId = await CreateOwner();
        var petId = await CreatePet(ownerId, "Rex", "Dog");

        var response = await _client.DeleteAsync($"/v1/pets/{petId}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_When_PetNotFound_Returns_NotFound()
    {
        var response = await _client.DeleteAsync($"/v1/pets/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private async Task<Guid> CreateOwner()
    {
        var response = await _client.PostAsJsonAsync("/v1/owners", new
        {
            firstName = "Test",
            lastName = "Owner",
            email = $"owner-{Guid.NewGuid():N}@example.com",
            phoneNumber = (string?)null
        });

        response.EnsureSuccessStatusCode();
        var location = response.Headers.Location!.ToString();
        return Guid.Parse(location.Split('/').Last());
    }

    private async Task<Guid> CreatePet(Guid ownerId, string name, string petType)
    {
        var response = await _client.PostAsJsonAsync("/v1/pets", new
        {
            ownerId,
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
