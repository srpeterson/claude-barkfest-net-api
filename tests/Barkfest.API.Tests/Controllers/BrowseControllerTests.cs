using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Barkfest.API.Tests.Controllers;

public class BrowseControllerTests(BarkfestApiFactory factory)
    : IClassFixture<BarkfestApiFactory>
{
    private readonly HttpClient _httpClient = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    private async Task<Guid> RegisterOwnerAndAddPetImageAsync()
    {
        var registerResponse = await _httpClient.PostAsJsonAsync("/v1/auth/register", new
        {
            username = $"browse{Guid.NewGuid():N}",
            firstName = "Alice",
            lastName = "Adams",
            email = $"browse-{Guid.NewGuid():N}@example.com",
            phoneNumber = (string?)null,
            password = "SecurePass1!"
        });

        var ownerId = Guid.Parse(registerResponse.Headers.Location!.ToString().Split('/').Last());

        var authClient = factory.CreateAuthenticatedClient(ownerId);

        var petResponse = await authClient.PostAsJsonAsync("/v1/pets", new
        {
            name = "Buddy",
            description = (string?)null,
            dateOfBirth = (string?)null,
            petType = "Dog",
            breed = (string?)null
        });

        petResponse.EnsureSuccessStatusCode();
        var petId = Guid.Parse(petResponse.Headers.Location!.ToString().Split('/').Last());

        using var imageContent = new MultipartFormDataContent();
        var imageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        imageContent.Add(new ByteArrayContent(imageBytes) { Headers = { ContentType = new("image/jpeg") } }, "file", "photo.jpg");
        await authClient.PostAsync($"/v1/pets/{petId}/images", imageContent);

        return ownerId;
    }

    // -----------------------------------------------------------------------
    // GET /v1/browse/images
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetImages_When_Unauthenticated_Returns_Ok()
    {
        var response = await _httpClient.GetAsync("/v1/browse/images");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetImages_When_NoFilters_Returns_Ok_WithImages()
    {
        await RegisterOwnerAndAddPetImageAsync();

        var response = await _httpClient.GetAsync("/v1/browse/images");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetArrayLength().ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetImages_When_FilteredByPetType_Returns_Ok()
    {
        var response = await _httpClient.GetAsync("/v1/browse/images?petType=Dog");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        foreach (var item in doc.RootElement.EnumerateArray())
            item.GetProperty("petType").GetString().ShouldBe("Dog");
    }

    [Fact]
    public async Task GetImages_When_UnrecognisedPetType_Returns_Ok_WithEmptyArray()
    {
        var response = await _httpClient.GetAsync("/v1/browse/images?petType=Dragon");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetArrayLength().ShouldBe(0);
    }

    [Fact]
    public async Task GetImages_When_ResponseContainsImage_Includes_OwnerName_And_PetProperties()
    {
        await RegisterOwnerAndAddPetImageAsync();

        var response = await _httpClient.GetAsync("/v1/browse/images");
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        var first = doc.RootElement.EnumerateArray().First();
        first.TryGetProperty("ownerName", out _).ShouldBeTrue();
        first.TryGetProperty("petName", out _).ShouldBeTrue();
        first.TryGetProperty("petType", out _).ShouldBeTrue();
        first.TryGetProperty("blobName", out _).ShouldBeTrue();
    }
}
