using System.Net;
using System.Net.Http.Json;

namespace Barkfest.Integration.Tests.Owners;

public class OwnerLifecycleTests(IntegrationApiFactory factory)
    : IClassFixture<IntegrationApiFactory>
{
    private readonly HttpClient _unauthenticatedClient = factory.CreateClient();

    // -----------------------------------------------------------------------
    // Full owner CRUD lifecycle
    // -----------------------------------------------------------------------

    [Fact]
    public async Task OwnerCrudLifecycle_CreateUpdateGetDelete_Succeeds()
    {
        // Register
        var (client, id) = await RegisterAndGetClient();

        // Get
        var getResponse = await client.GetAsync($"/v1/owners/{id}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await getResponse.Content.ReadAsStringAsync();
        body.ShouldContain("Integration");

        // Update
        var updateResponse = await client.PutAsJsonAsync($"/v1/owners/{id}", new
        {
            firstName = "Updated",
            lastName = "Tester",
            email = $"updated-{Guid.NewGuid():N}@example.com",
            phoneNumber = (string?)null
        });
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify update
        var getAfterUpdate = await client.GetAsync($"/v1/owners/{id}");
        var updatedBody = await getAfterUpdate.Content.ReadAsStringAsync();
        updatedBody.ShouldContain("Updated");

        // Delete
        var deleteResponse = await client.DeleteAsync($"/v1/owners/{id}");
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify gone
        var getAfterDelete = await client.GetAsync($"/v1/owners/{id}");
        getAfterDelete.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // -----------------------------------------------------------------------
    // Profile image upload / remove
    // -----------------------------------------------------------------------

    [Fact]
    public async Task UploadProfileImage_When_JpegProvided_Returns_NoContent()
    {
        var (client, id) = await RegisterAndGetClient();

        var response = await client.PostAsync(
            $"/v1/owners/{id}/profile-image",
            BuildImageFormData("profile.jpg", "image/jpeg"));

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UploadProfileImage_When_PngProvided_Returns_NoContent()
    {
        var (client, id) = await RegisterAndGetClient();

        var response = await client.PostAsync(
            $"/v1/owners/{id}/profile-image",
            BuildImageFormData("profile.png", "image/png"));

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UploadProfileImage_When_OwnerNotFound_Returns_NotFound()
    {
        var (client, _) = await RegisterAndGetClient();

        var response = await client.PostAsync(
            $"/v1/owners/{Guid.NewGuid()}/profile-image",
            BuildImageFormData("profile.jpg", "image/jpeg"));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveProfileImage_When_PreviouslyUploaded_Returns_NoContent()
    {
        var (client, id) = await RegisterAndGetClient();

        var uploadResponse = await client.PostAsync(
            $"/v1/owners/{id}/profile-image",
            BuildImageFormData("profile.jpg", "image/jpeg"));
        uploadResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var removeResponse = await client.DeleteAsync($"/v1/owners/{id}/profile-image");
        removeResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UploadProfileImage_When_ImageAlreadyExists_Replaces_Image()
    {
        var (client, id) = await RegisterAndGetClient();

        // First upload
        var first = await client.PostAsync(
            $"/v1/owners/{id}/profile-image",
            BuildImageFormData("first.jpg", "image/jpeg"));
        first.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Second upload — replaces first
        var second = await client.PostAsync(
            $"/v1/owners/{id}/profile-image",
            BuildImageFormData("second.jpg", "image/jpeg"));
        second.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveProfileImage_When_NoImageSet_Returns_NoContent()
    {
        var (client, id) = await RegisterAndGetClient();

        // The handler is idempotent — removing a non-existent profile image is a no-op
        // that still returns 204 NoContent, not 404.
        var response = await client.DeleteAsync($"/v1/owners/{id}/profile-image");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    // -----------------------------------------------------------------------
    // Owner → Pets relationship
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetPets_When_PetsExist_Returns_Pets()
    {
        var (client, ownerId) = await RegisterAndGetClient();

        // Create two pets
        await client.PostAsJsonAsync("/v1/pets", new
        {
            name = "Fido",
            description = (string?)null,
            dateOfBirth = (string?)null,
            petType = "Dog"
        });
        await client.PostAsJsonAsync("/v1/pets", new
        {
            name = "Whiskers",
            description = (string?)null,
            dateOfBirth = (string?)null,
            petType = "Cat"
        });

        var response = await client.GetAsync($"/v1/owners/{ownerId}/pets");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("Fido");
        body.ShouldContain("Whiskers");
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private async Task<(HttpClient client, Guid ownerId)> RegisterAndGetClient()
    {
        var registerResponse = await _unauthenticatedClient.PostAsJsonAsync("/v1/auth/register", new
        {
            username = $"integration{Guid.NewGuid():N}",
            firstName = "Integration",
            lastName = "Tester",
            email = $"integration-{Guid.NewGuid():N}@example.com",
            phoneNumber = (string?)null,
            password = "SecurePass1!"
        });
        registerResponse.EnsureSuccessStatusCode();

        var location = registerResponse.Headers.Location!.ToString();
        var ownerId = Guid.Parse(location.Split('/').Last());

        return (factory.CreateAuthenticatedClient(ownerId), ownerId);
    }

    private static Guid ExtractIdFromLocation(HttpResponseMessage response) =>
        Guid.Parse(response.Headers.Location!.ToString().Split('/').Last());

    private static MultipartFormDataContent BuildImageFormData(string fileName, string contentType)
    {
        // A minimal JPEG SOI marker — enough bytes for the handler to open the stream.
        var fakeImageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 };
        var content = new ByteArrayContent(fakeImageBytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        return new MultipartFormDataContent
        {
            { content, "file", fileName }
        };
    }
}
