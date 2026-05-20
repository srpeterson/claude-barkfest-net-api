using Barkfest.Domain.Entities;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Barkfest.Integration.Tests.Pets;

public class PetLifecycleTests(IntegrationApiFactory factory)
    : IClassFixture<IntegrationApiFactory>
{
    private readonly HttpClient _unauthenticatedClient = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    // -----------------------------------------------------------------------
    // Full pet CRUD lifecycle
    // -----------------------------------------------------------------------

    [Fact]
    public async Task PetCrudLifecycle_CreateUpdateGetDelete_Succeeds()
    {
        var (client, _) = await RegisterAndGetClient();

        // Create
        var createResponse = await client.PostAsJsonAsync("/v1/pets", new
        {
            name = "Bruno",
            description = "A very good boy",
            dateOfBirth = (string?)null,
            petType = "Dog"
        });
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var petId = ExtractIdFromLocation(createResponse);

        // Get
        var getResponse = await client.GetAsync($"/v1/pets/{petId}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await getResponse.Content.ReadAsStringAsync();
        body.ShouldContain("Bruno");

        // Update
        var updateResponse = await client.PutAsJsonAsync($"/v1/pets/{petId}", new
        {
            name = "Bruno Jr.",
            description = "Still a very good boy",
            dateOfBirth = (string?)null,
            petType = "Dog"
        });
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify update
        var getAfterUpdate = await client.GetAsync($"/v1/pets/{petId}");
        var updatedBody = await getAfterUpdate.Content.ReadAsStringAsync();
        updatedBody.ShouldContain("Bruno Jr.");

        // Delete
        var deleteResponse = await client.DeleteAsync($"/v1/pets/{petId}");
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify gone
        var getAfterDelete = await client.GetAsync($"/v1/pets/{petId}");
        getAfterDelete.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // -----------------------------------------------------------------------
    // Profile image upload / remove
    // -----------------------------------------------------------------------

    [Fact]
    public async Task UploadProfileImage_When_JpegProvided_Returns_NoContent()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Rex");

        var response = await client.PostAsync(
            $"/v1/pets/{petId}/profile-image",
            BuildImageFormData("pet-profile.jpg", "image/jpeg"));

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UploadProfileImage_When_PetNotFound_Returns_NotFound()
    {
        var (client, _) = await RegisterAndGetClient();

        var response = await client.PostAsync(
            $"/v1/pets/{Guid.NewGuid()}/profile-image",
            BuildImageFormData("pet-profile.jpg", "image/jpeg"));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveProfileImage_When_PreviouslyUploaded_Returns_NoContent()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Bella");

        var uploadResponse = await client.PostAsync(
            $"/v1/pets/{petId}/profile-image",
            BuildImageFormData("bella.jpg", "image/jpeg"));
        uploadResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var removeResponse = await client.DeleteAsync($"/v1/pets/{petId}/profile-image");
        removeResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UploadProfileImage_When_ImageAlreadyExists_Replaces_Image()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Max");

        var first = await client.PostAsync(
            $"/v1/pets/{petId}/profile-image",
            BuildImageFormData("first.jpg", "image/jpeg"));
        first.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var second = await client.PostAsync(
            $"/v1/pets/{petId}/profile-image",
            BuildImageFormData("second.png", "image/png"));
        second.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    // -----------------------------------------------------------------------
    // Gallery images (add / remove)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AddImage_When_FileIsValid_Returns_Created()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Luna");

        var response = await client.PostAsync(
            $"/v1/pets/{petId}/images",
            BuildImageFormData("gallery1.jpg", "image/jpeg"));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("imageId");
    }

    [Fact]
    public async Task AddImage_When_PetNotFound_Returns_NotFound()
    {
        var (client, _) = await RegisterAndGetClient();

        var response = await client.PostAsync(
            $"/v1/pets/{Guid.NewGuid()}/images",
            BuildImageFormData("gallery1.jpg", "image/jpeg"));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveImage_When_ImageExists_Returns_NoContent()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Charlie");

        var addResponse = await client.PostAsync(
            $"/v1/pets/{petId}/images",
            BuildImageFormData("gallery1.jpg", "image/jpeg"));
        addResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var imageId = await ExtractImageId(addResponse);

        var removeResponse = await client.DeleteAsync($"/v1/pets/{petId}/images/{imageId}");
        removeResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveImage_When_ImageNotFound_Returns_NotFound()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Daisy");

        var response = await client.DeleteAsync($"/v1/pets/{petId}/images/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddImage_When_UpToMaxLimit_AllSucceed()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Archie");

        for (var i = 1; i <= Pet.MaxImages; i++)
        {
            var response = await client.PostAsync(
                $"/v1/pets/{petId}/images",
                BuildImageFormData($"gallery{i}.jpg", "image/jpeg"));

            response.StatusCode.ShouldBe(HttpStatusCode.Created, $"image {i} should succeed");
        }
    }

    [Fact]
    public async Task AddImage_When_ExceedsMaxLimit_Returns_BadRequest()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Milo");

        for (var i = 1; i <= Pet.MaxImages; i++)
        {
            await client.PostAsync(
                $"/v1/pets/{petId}/images",
                BuildImageFormData($"gallery{i}.jpg", "image/jpeg"));
        }

        // One more image should exceed the domain limit
        var overLimit = await client.PostAsync(
            $"/v1/pets/{petId}/images",
            BuildImageFormData($"gallery{Pet.MaxImages + 1}.jpg", "image/jpeg"));

        overLimit.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    // -----------------------------------------------------------------------
    // Full cross-resource lifecycle
    // -----------------------------------------------------------------------

    [Fact]
    public async Task FullLifecycle_OwnerWithPetAndImages_Succeeds()
    {
        var (client, ownerId) = await RegisterAndGetClient();

        // Upload owner profile image
        var ownerImageUpload = await client.PostAsync(
            $"/v1/owners/{ownerId}/profile-image",
            BuildImageFormData("owner.jpg", "image/jpeg"));
        ownerImageUpload.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Create pet under that owner
        var petId = await CreatePet(client, "Rocket");

        // Upload pet profile image
        var petImageUpload = await client.PostAsync(
            $"/v1/pets/{petId}/profile-image",
            BuildImageFormData("rocket.jpg", "image/jpeg"));
        petImageUpload.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Add two gallery images
        var img1Response = await client.PostAsync(
            $"/v1/pets/{petId}/images",
            BuildImageFormData("rocket-play.jpg", "image/jpeg"));
        img1Response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var img1Id = await ExtractImageId(img1Response);

        var img2Response = await client.PostAsync(
            $"/v1/pets/{petId}/images",
            BuildImageFormData("rocket-sleep.png", "image/png"));
        img2Response.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Remove first gallery image
        var removeImg1 = await client.DeleteAsync($"/v1/pets/{petId}/images/{img1Id}");
        removeImg1.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Get pet — should still contain gallery image 2
        var getPet = await client.GetAsync($"/v1/pets/{petId}");
        getPet.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Delete pet
        var deletePet = await client.DeleteAsync($"/v1/pets/{petId}");
        deletePet.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify pet gone
        (await client.GetAsync($"/v1/pets/{petId}")).StatusCode
            .ShouldBe(HttpStatusCode.NotFound);

        // Delete owner
        var deleteOwner = await client.DeleteAsync($"/v1/owners/{ownerId}");
        deleteOwner.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private async Task<(HttpClient client, Guid ownerId)> RegisterAndGetClient()
    {
        var registerResponse = await _unauthenticatedClient.PostAsJsonAsync("/v1/auth/register", new
        {
            username = $"testowner{Guid.NewGuid():N}",
            firstName = "Test",
            lastName = "Owner",
            email = $"owner-{Guid.NewGuid():N}@example.com",
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
        return ExtractIdFromLocation(response);
    }

    private static Guid ExtractIdFromLocation(HttpResponseMessage response) =>
        Guid.Parse(response.Headers.Location!.ToString().Split('/').Last());

    private static async Task<Guid> ExtractImageId(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("imageId").GetGuid();
    }

    private static MultipartFormDataContent BuildImageFormData(string fileName, string contentType)
    {
        var fakeImageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 }; // JPEG SOI marker
        var content = new ByteArrayContent(fakeImageBytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        var form = new MultipartFormDataContent();
        form.Add(content, "file", fileName);
        return form;
    }
}
