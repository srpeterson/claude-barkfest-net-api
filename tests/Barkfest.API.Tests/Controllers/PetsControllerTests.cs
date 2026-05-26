using Barkfest.Domain.Entities;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

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
            petTypeValue = 1,
            breedValue = 7
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
            petTypeValue = 1,
            breedValue = 7
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
            petTypeValue = 1,
            breedValue = 7
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
            petTypeValue = 1,
            breedValue = 3
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
            petTypeValue = 1,
            breedValue = 7
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
    // POST /v1/pets/{id}/images — batch upload
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AddImages_When_Unauthenticated_Returns_Unauthorized()
    {
        var response = await _unauthenticatedClient.PostAsync(
            $"/v1/pets/{Guid.NewGuid()}/images",
            BuildBatchFormData([("photo.jpg", "image/jpeg")]));

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddImages_When_FilesAreValid_Returns_Created()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Luna");

        var response = await client.PostAsync(
            $"/v1/pets/{petId}/images",
            BuildBatchFormData([("gallery1.jpg", "image/jpeg"), ("gallery2.png", "image/png")]));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("results");
    }

    [Fact]
    public async Task AddImages_When_PetNotFound_Returns_NotFound()
    {
        var (client, _) = await RegisterAndGetClient();

        var response = await client.PostAsync(
            $"/v1/pets/{Guid.NewGuid()}/images",
            BuildBatchFormData([("gallery1.jpg", "image/jpeg")]));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddImages_When_ExceedsAvailableSlots_Returns_BadRequest()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Luna");

        for (var i = 0; i < Pet.MaxImages; i++)
            await client.PostAsync($"/v1/pets/{petId}/images",
                BuildBatchFormData([($"gallery{i}.jpg", "image/jpeg")]));

        var response = await client.PostAsync(
            $"/v1/pets/{petId}/images",
            BuildBatchFormData([("extra.jpg", "image/jpeg")]));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    // -----------------------------------------------------------------------
    // POST /v1/pets/{id}/images/batch-delete
    // -----------------------------------------------------------------------

    [Fact]
    public async Task BatchDeleteImages_When_AllExist_Returns_NoContent()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Buddy");
        var imageId = await AddImageAndGetId(client, petId);

        var response = await client.PostAsJsonAsync(
            $"/v1/pets/{petId}/images/batch-delete",
            new { imageIds = new[] { imageId } });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task BatchDeleteImages_When_AnyNotFound_Returns_BadRequest()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Buddy");
        var imageId = await AddImageAndGetId(client, petId);

        var response = await client.PostAsJsonAsync(
            $"/v1/pets/{petId}/images/batch-delete",
            new { imageIds = new[] { imageId, Guid.NewGuid() } });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task BatchDeleteImages_When_EmptyList_Returns_BadRequest()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Buddy");

        var response = await client.PostAsJsonAsync(
            $"/v1/pets/{petId}/images/batch-delete",
            new { imageIds = Array.Empty<Guid>() });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    // -----------------------------------------------------------------------
    // PUT /v1/pets/{id}/images/{imageId}/featured
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SetFeaturedImage_When_ImageExists_Returns_NoContent()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Buddy");
        var imageId = await AddImageAndGetId(client, petId);

        var response = await client.PutAsync($"/v1/pets/{petId}/images/{imageId}/featured", null);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SetFeaturedImage_When_ImageNotFound_Returns_BadRequest()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Buddy");

        var response = await client.PutAsync($"/v1/pets/{petId}/images/{Guid.NewGuid()}/featured", null);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
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

    private async Task<Guid> CreatePet(HttpClient client, string name, int petTypeValue = 1, int breedValue = 7)
    {
        var response = await client.PostAsJsonAsync("/v1/pets", new
        {
            name,
            description = (string?)null,
            dateOfBirth = (string?)null,
            petTypeValue,
            breedValue
        });

        response.EnsureSuccessStatusCode();
        var location = response.Headers.Location!.ToString();
        return Guid.Parse(location.Split('/').Last());
    }

    private async Task<Guid> AddImageAndGetId(HttpClient client, Guid petId)
    {
        var response = await client.PostAsync(
            $"/v1/pets/{petId}/images",
            BuildBatchFormData([("photo.jpg", "image/jpeg")]));
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement
            .GetProperty("results")[0]
            .GetProperty("imageId")
            .GetGuid();
    }

    private static MultipartFormDataContent BuildBatchFormData(
        IEnumerable<(string FileName, string ContentType)> files)
    {
        var fakeImageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 };
        var form = new MultipartFormDataContent();

        foreach (var (fileName, contentType) in files)
        {
            var content = new ByteArrayContent(fakeImageBytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            form.Add(content, "files", fileName);
        }

        return form;
    }
}
