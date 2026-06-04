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
            petTypeValue = 1,
            breedValue = 7
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
            petTypeValue = 1,
            breedValue = 3
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
    // Set featured image
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SetFeaturedImage_When_ImageExists_Returns_NoContent()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Rex");
        var imageId = await AddImageAndGetId(client, petId, "rex.jpg");

        var response = await client.PutAsync(
            $"/v1/pets/{petId}/images/{imageId}/featured", null);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SetFeaturedImage_When_ReplacingExisting_Returns_NoContent()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Bella");
        var firstImageId = await AddImageAndGetId(client, petId, "bella1.jpg");
        var secondImageId = await AddImageAndGetId(client, petId, "bella2.jpg");

        await client.PutAsync($"/v1/pets/{petId}/images/{firstImageId}/featured", null);
        var response = await client.PutAsync(
            $"/v1/pets/{petId}/images/{secondImageId}/featured", null);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    // -----------------------------------------------------------------------
    // Batch upload images
    // -----------------------------------------------------------------------

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
    public async Task AddImages_When_UpToMaxLimit_AllSucceed()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Archie");

        for (var i = 1; i <= Pet.MaxImages; i++)
        {
            var response = await client.PostAsync(
                $"/v1/pets/{petId}/images",
                BuildBatchFormData([($"gallery{i}.jpg", "image/jpeg")]));

            response.StatusCode.ShouldBe(HttpStatusCode.Created, $"image {i} should succeed");
        }
    }

    [Fact]
    public async Task AddImages_When_ExceedsMaxLimit_Returns_BadRequest()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Milo");

        for (var i = 1; i <= Pet.MaxImages; i++)
        {
            await client.PostAsync(
                $"/v1/pets/{petId}/images",
                BuildBatchFormData([($"gallery{i}.jpg", "image/jpeg")]));
        }

        var overLimit = await client.PostAsync(
            $"/v1/pets/{petId}/images",
            BuildBatchFormData([($"gallery{Pet.MaxImages + 1}.jpg", "image/jpeg")]));

        overLimit.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddImages_When_FirstUpload_FirstImage_IsAutoFeatured()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Nova");

        var uploadResponse = await client.PostAsync(
            $"/v1/pets/{petId}/images",
            BuildBatchFormData([("nova1.jpg", "image/jpeg"), ("nova2.jpg", "image/jpeg")]));
        uploadResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var getPet = await client.GetAsync($"/v1/pets/{petId}");
        var petBody = await getPet.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(petBody);
        var images = doc.RootElement.GetProperty("images");
        var featured = images.EnumerateArray().FirstOrDefault(i => i.GetProperty("isFeaturedImage").GetBoolean());
        featured.ValueKind.ShouldNotBe(JsonValueKind.Undefined);
    }

    // -----------------------------------------------------------------------
    // Batch delete images
    // -----------------------------------------------------------------------

    [Fact]
    public async Task BatchDeleteImages_When_AllExist_Returns_NoContent()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Charlie");
        var imageId1 = await AddImageAndGetId(client, petId, "charlie1.jpg");
        var imageId2 = await AddImageAndGetId(client, petId, "charlie2.jpg");

        var response = await client.PostAsJsonAsync(
            $"/v1/pets/{petId}/images/batch-delete",
            new { imageIds = new[] { imageId1, imageId2 } });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify both gone
        var getPet = await client.GetAsync($"/v1/pets/{petId}");
        var body = await getPet.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("images").GetArrayLength().ShouldBe(0);
    }

    [Fact]
    public async Task BatchDeleteImages_When_AnyNotFound_Returns_BadRequest()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Daisy");
        var imageId = await AddImageAndGetId(client, petId, "daisy.jpg");

        var response = await client.PostAsJsonAsync(
            $"/v1/pets/{petId}/images/batch-delete",
            new { imageIds = new[] { imageId, Guid.NewGuid() } });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    // -----------------------------------------------------------------------
    // Single image remove
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RemoveImage_When_ImageExists_Returns_NoContent()
    {
        var (client, _) = await RegisterAndGetClient();
        var petId = await CreatePet(client, "Charlie");
        var imageId = await AddImageAndGetId(client, petId, "gallery1.jpg");

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
            BuildSingleFormData("owner.jpg", "image/jpeg"));
        ownerImageUpload.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Create pet under that owner
        var petId = await CreatePet(client, "Rocket");

        // Add two gallery images as a batch
        var batchResponse = await client.PostAsync(
            $"/v1/pets/{petId}/images",
            BuildBatchFormData([("rocket-play.jpg", "image/jpeg"), ("rocket-sleep.png", "image/png")]));
        batchResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var batchBody = await batchResponse.Content.ReadAsStringAsync();
        using var batchDoc = JsonDocument.Parse(batchBody);
        var img1Id = batchDoc.RootElement
            .GetProperty("results")[0]
            .GetProperty("imageId")
            .GetGuid();

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
        return ExtractIdFromLocation(response);
    }

    private static Guid ExtractIdFromLocation(HttpResponseMessage response) =>
        Guid.Parse(response.Headers.Location!.ToString().Split('/').Last());

    private async Task<Guid> AddImageAndGetId(HttpClient client, Guid petId, string fileName)
    {
        var response = await client.PostAsync(
            $"/v1/pets/{petId}/images",
            BuildBatchFormData([(fileName, "image/jpeg")]));
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

    private static MultipartFormDataContent BuildSingleFormData(string fileName, string contentType)
    {
        var fakeImageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 };
        var content = new ByteArrayContent(fakeImageBytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        var form = new MultipartFormDataContent();
        form.Add(content, "file", fileName);
        return form;
    }
}
