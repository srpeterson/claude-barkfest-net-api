using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Barkfest.Integration.Tests.Pets;

public class PetLifecycleTests(IntegrationApiFactory factory)
    : IClassFixture<IntegrationApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    // -----------------------------------------------------------------------
    // Full pet CRUD lifecycle
    // -----------------------------------------------------------------------

    [Fact]
    public async Task PetCrudLifecycle_CreateUpdateGetDelete_Succeeds()
    {
        var ownerId = await CreateOwner();

        // Create
        var createResponse = await _client.PostAsJsonAsync("/api/pets", new
        {
            ownerId,
            name = "Bruno",
            description = "A very good boy",
            dateOfBirth = (string?)null,
            petType = "Dog"
        });
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var petId = ExtractIdFromLocation(createResponse);

        // Get
        var getResponse = await _client.GetAsync($"/api/pets/{petId}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await getResponse.Content.ReadAsStringAsync();
        body.ShouldContain("Bruno");

        // Update
        var updateResponse = await _client.PutAsJsonAsync($"/api/pets/{petId}", new
        {
            name = "Bruno Jr.",
            description = "Still a very good boy",
            dateOfBirth = (string?)null,
            petType = "Dog"
        });
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify update
        var getAfterUpdate = await _client.GetAsync($"/api/pets/{petId}");
        var updatedBody = await getAfterUpdate.Content.ReadAsStringAsync();
        updatedBody.ShouldContain("Bruno Jr.");

        // Delete
        var deleteResponse = await _client.DeleteAsync($"/api/pets/{petId}");
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify gone
        var getAfterDelete = await _client.GetAsync($"/api/pets/{petId}");
        getAfterDelete.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // -----------------------------------------------------------------------
    // Profile image upload / remove
    // -----------------------------------------------------------------------

    [Fact]
    public async Task UploadProfileImage_ValidJpeg_Returns204()
    {
        var ownerId = await CreateOwner();
        var petId = await CreatePet(ownerId, "Rex", "Dog");

        var response = await _client.PostAsync(
            $"/api/pets/{petId}/profile-image",
            BuildImageFormData("pet-profile.jpg", "image/jpeg"));

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UploadProfileImage_UnknownPet_Returns404()
    {
        var response = await _client.PostAsync(
            $"/api/pets/{Guid.NewGuid()}/profile-image",
            BuildImageFormData("pet-profile.jpg", "image/jpeg"));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UploadThenRemoveProfileImage_Returns204BothTimes()
    {
        var ownerId = await CreateOwner();
        var petId = await CreatePet(ownerId, "Bella", "Cat");

        var uploadResponse = await _client.PostAsync(
            $"/api/pets/{petId}/profile-image",
            BuildImageFormData("bella.jpg", "image/jpeg"));
        uploadResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var removeResponse = await _client.DeleteAsync($"/api/pets/{petId}/profile-image");
        removeResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UploadProfileImage_ReplacesExistingImage()
    {
        var ownerId = await CreateOwner();
        var petId = await CreatePet(ownerId, "Max", "Dog");

        var first = await _client.PostAsync(
            $"/api/pets/{petId}/profile-image",
            BuildImageFormData("first.jpg", "image/jpeg"));
        first.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var second = await _client.PostAsync(
            $"/api/pets/{petId}/profile-image",
            BuildImageFormData("second.png", "image/png"));
        second.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    // -----------------------------------------------------------------------
    // Gallery images (add / remove)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AddImage_ValidFile_Returns201WithImageId()
    {
        var ownerId = await CreateOwner();
        var petId = await CreatePet(ownerId, "Luna", "Cat");

        var response = await _client.PostAsync(
            $"/api/pets/{petId}/images",
            BuildImageFormData("gallery1.jpg", "image/jpeg"));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("imageId");
    }

    [Fact]
    public async Task AddImage_UnknownPet_Returns404()
    {
        var response = await _client.PostAsync(
            $"/api/pets/{Guid.NewGuid()}/images",
            BuildImageFormData("gallery1.jpg", "image/jpeg"));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddThenRemoveImage_Returns204OnRemove()
    {
        var ownerId = await CreateOwner();
        var petId = await CreatePet(ownerId, "Charlie", "Dog");

        var addResponse = await _client.PostAsync(
            $"/api/pets/{petId}/images",
            BuildImageFormData("gallery1.jpg", "image/jpeg"));
        addResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var imageId = await ExtractImageId(addResponse);

        var removeResponse = await _client.DeleteAsync($"/api/pets/{petId}/images/{imageId}");
        removeResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveImage_UnknownImageId_Returns404()
    {
        var ownerId = await CreateOwner();
        var petId = await CreatePet(ownerId, "Daisy", "Dog");

        var response = await _client.DeleteAsync($"/api/pets/{petId}/images/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddMultipleImages_UpToMaxLimit_AllSucceed()
    {
        var ownerId = await CreateOwner();
        var petId = await CreatePet(ownerId, "Archie", "Dog");

        // MaxImages == 5
        for (var i = 1; i <= 5; i++)
        {
            var response = await _client.PostAsync(
                $"/api/pets/{petId}/images",
                BuildImageFormData($"gallery{i}.jpg", "image/jpeg"));

            response.StatusCode.ShouldBe(HttpStatusCode.Created, $"image {i} should succeed");
        }
    }

    [Fact]
    public async Task AddImage_ExceedsMaxLimit_Returns400()
    {
        var ownerId = await CreateOwner();
        var petId = await CreatePet(ownerId, "Milo", "Dog");

        for (var i = 1; i <= 5; i++)
        {
            await _client.PostAsync(
                $"/api/pets/{petId}/images",
                BuildImageFormData($"gallery{i}.jpg", "image/jpeg"));
        }

        // 6th image should exceed the domain limit
        var overLimit = await _client.PostAsync(
            $"/api/pets/{petId}/images",
            BuildImageFormData("gallery6.jpg", "image/jpeg"));

        overLimit.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    // -----------------------------------------------------------------------
    // Full cross-resource lifecycle
    // -----------------------------------------------------------------------

    [Fact]
    public async Task FullLifecycle_OwnerWithPetAndImages_Succeeds()
    {
        // 1. Create owner
        var ownerId = await CreateOwner();

        // 2. Upload owner profile image
        var ownerImageUpload = await _client.PostAsync(
            $"/api/owners/{ownerId}/profile-image",
            BuildImageFormData("owner.jpg", "image/jpeg"));
        ownerImageUpload.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // 3. Create pet under that owner
        var petId = await CreatePet(ownerId, "Rocket", "Dog");

        // 4. Upload pet profile image
        var petImageUpload = await _client.PostAsync(
            $"/api/pets/{petId}/profile-image",
            BuildImageFormData("rocket.jpg", "image/jpeg"));
        petImageUpload.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // 5. Add two gallery images
        var img1Response = await _client.PostAsync(
            $"/api/pets/{petId}/images",
            BuildImageFormData("rocket-play.jpg", "image/jpeg"));
        img1Response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var img1Id = await ExtractImageId(img1Response);

        var img2Response = await _client.PostAsync(
            $"/api/pets/{petId}/images",
            BuildImageFormData("rocket-sleep.png", "image/png"));
        img2Response.StatusCode.ShouldBe(HttpStatusCode.Created);

        // 6. Remove first gallery image
        var removeImg1 = await _client.DeleteAsync($"/api/pets/{petId}/images/{img1Id}");
        removeImg1.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // 7. Get pet — should still contain gallery image 2
        var getPet = await _client.GetAsync($"/api/pets/{petId}");
        getPet.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 8. Delete pet
        var deletePet = await _client.DeleteAsync($"/api/pets/{petId}");
        deletePet.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // 9. Verify pet gone
        (await _client.GetAsync($"/api/pets/{petId}")).StatusCode
            .ShouldBe(HttpStatusCode.NotFound);

        // 10. Delete owner
        var deleteOwner = await _client.DeleteAsync($"/api/owners/{ownerId}");
        deleteOwner.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private async Task<Guid> CreateOwner()
    {
        var response = await _client.PostAsJsonAsync("/api/owners", new
        {
            firstName = "Test",
            lastName = "Owner",
            email = $"owner-{Guid.NewGuid():N}@example.com",
            phoneNumber = (string?)null
        });
        response.EnsureSuccessStatusCode();
        return ExtractIdFromLocation(response);
    }

    private async Task<Guid> CreatePet(Guid ownerId, string name, string petType)
    {
        var response = await _client.PostAsJsonAsync("/api/pets", new
        {
            ownerId,
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
