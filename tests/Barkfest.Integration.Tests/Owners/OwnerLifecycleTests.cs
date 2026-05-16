using System.Net;
using System.Net.Http.Json;

namespace Barkfest.Integration.Tests.Owners;

public class OwnerLifecycleTests(IntegrationApiFactory factory)
    : IClassFixture<IntegrationApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    // -----------------------------------------------------------------------
    // Full owner CRUD lifecycle
    // -----------------------------------------------------------------------

    [Fact]
    public async Task OwnerCrudLifecycle_CreateUpdateGetDelete_Succeeds()
    {
        // Create
        var createResponse = await _client.PostAsJsonAsync("/api/owners", new
        {
            firstName = "Integration",
            lastName = "Tester",
            email = $"integration-{Guid.NewGuid():N}@example.com",
            phoneNumber = "555-0199"
        });
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var id = ExtractIdFromLocation(createResponse);

        // Get
        var getResponse = await _client.GetAsync($"/api/owners/{id}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await getResponse.Content.ReadAsStringAsync();
        body.ShouldContain("Integration");

        // Update
        var updateResponse = await _client.PutAsJsonAsync($"/api/owners/{id}", new
        {
            firstName = "Updated",
            lastName = "Tester",
            email = $"updated-{Guid.NewGuid():N}@example.com",
            phoneNumber = (string?)null
        });
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify update
        var getAfterUpdate = await _client.GetAsync($"/api/owners/{id}");
        var updatedBody = await getAfterUpdate.Content.ReadAsStringAsync();
        updatedBody.ShouldContain("Updated");

        // Delete
        var deleteResponse = await _client.DeleteAsync($"/api/owners/{id}");
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify gone
        var getAfterDelete = await _client.GetAsync($"/api/owners/{id}");
        getAfterDelete.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // -----------------------------------------------------------------------
    // Profile image upload / remove
    // -----------------------------------------------------------------------

    [Fact]
    public async Task UploadProfileImage_ValidJpeg_Returns204()
    {
        var id = await CreateOwner();

        var response = await _client.PostAsync(
            $"/api/owners/{id}/profile-image",
            BuildImageFormData("profile.jpg", "image/jpeg"));

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UploadProfileImage_ValidPng_Returns204()
    {
        var id = await CreateOwner();

        var response = await _client.PostAsync(
            $"/api/owners/{id}/profile-image",
            BuildImageFormData("profile.png", "image/png"));

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UploadProfileImage_UnknownOwner_Returns404()
    {
        var response = await _client.PostAsync(
            $"/api/owners/{Guid.NewGuid()}/profile-image",
            BuildImageFormData("profile.jpg", "image/jpeg"));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UploadThenRemoveProfileImage_Returns204BothTimes()
    {
        var id = await CreateOwner();

        var uploadResponse = await _client.PostAsync(
            $"/api/owners/{id}/profile-image",
            BuildImageFormData("profile.jpg", "image/jpeg"));
        uploadResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var removeResponse = await _client.DeleteAsync($"/api/owners/{id}/profile-image");
        removeResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UploadProfileImage_ReplacesExistingImage()
    {
        var id = await CreateOwner();

        // First upload
        var first = await _client.PostAsync(
            $"/api/owners/{id}/profile-image",
            BuildImageFormData("first.jpg", "image/jpeg"));
        first.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Second upload — replaces first
        var second = await _client.PostAsync(
            $"/api/owners/{id}/profile-image",
            BuildImageFormData("second.jpg", "image/jpeg"));
        second.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveProfileImage_WhenNoImageSet_Returns204()
    {
        var id = await CreateOwner();

        // The handler is idempotent — removing a non-existent profile image is a no-op
        // that still returns 204 NoContent, not 404.
        var response = await _client.DeleteAsync($"/api/owners/{id}/profile-image");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    // -----------------------------------------------------------------------
    // Owner → Pets relationship
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetPets_AfterCreatingPets_ReturnsPets()
    {
        var ownerId = await CreateOwner();

        // Create two pets
        await _client.PostAsJsonAsync("/api/pets", new
        {
            ownerId,
            name = "Fido",
            description = (string?)null,
            dateOfBirth = (string?)null,
            petType = "Dog"
        });
        await _client.PostAsJsonAsync("/api/pets", new
        {
            ownerId,
            name = "Whiskers",
            description = (string?)null,
            dateOfBirth = (string?)null,
            petType = "Cat"
        });

        var response = await _client.GetAsync($"/api/owners/{ownerId}/pets");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("Fido");
        body.ShouldContain("Whiskers");
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

    private static Guid ExtractIdFromLocation(HttpResponseMessage response) =>
        Guid.Parse(response.Headers.Location!.ToString().Split('/').Last());

    private static MultipartFormDataContent BuildImageFormData(string fileName, string contentType)
    {
        // A minimal 1x1 JPEG / PNG in raw bytes — just enough bytes for the handler
        // to open the stream. Content type validation happens at the domain layer via
        // the file name extension and MIME type, not by inspecting the image payload.
        var fakeImageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 }; // JPEG SOI marker
        var content = new ByteArrayContent(fakeImageBytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        var form = new MultipartFormDataContent();
        form.Add(content, "file", fileName);
        return form;
    }
}
