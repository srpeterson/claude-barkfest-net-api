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

    private async Task<Guid> RegisterOwnerAndAddFeaturedPetImageAsync()
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
            breed = "Beagle"
        });

        petResponse.EnsureSuccessStatusCode();
        var petId = Guid.Parse(petResponse.Headers.Location!.ToString().Split('/').Last());

        using var imageContent = new MultipartFormDataContent();
        var imageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        imageContent.Add(
            new ByteArrayContent(imageBytes) { Headers = { ContentType = new("image/jpeg") } },
            "files", "photo.jpg");
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
    public async Task GetImages_When_NoFilters_Returns_Ok_WithPagedResult()
    {
        await RegisterOwnerAndAddFeaturedPetImageAsync();

        var response = await _httpClient.GetAsync("/v1/browse/images");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        doc.RootElement.TryGetProperty("items", out var items).ShouldBeTrue();
        doc.RootElement.TryGetProperty("page", out _).ShouldBeTrue();
        doc.RootElement.TryGetProperty("pageSize", out _).ShouldBeTrue();
        doc.RootElement.TryGetProperty("totalCount", out _).ShouldBeTrue();
        doc.RootElement.TryGetProperty("hasMore", out _).ShouldBeTrue();
        items.GetArrayLength().ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetImages_When_FilteredByPetType_Returns_Ok_WithMatchingItems()
    {
        var response = await _httpClient.GetAsync("/v1/browse/images?petType=Dog");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        foreach (var item in doc.RootElement.GetProperty("items").EnumerateArray())
            item.GetProperty("petType").GetString().ShouldBe("Dog");
    }

    [Fact]
    public async Task GetImages_When_UnrecognisedPetType_Returns_Ok_WithEmptyItems()
    {
        var response = await _httpClient.GetAsync("/v1/browse/images?petType=Dragon");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        doc.RootElement.GetProperty("items").GetArrayLength().ShouldBe(0);
        doc.RootElement.GetProperty("totalCount").GetInt32().ShouldBe(0);
    }

    [Fact]
    public async Task GetImages_When_ResponseContainsImage_Includes_OwnerName_And_PetProperties()
    {
        await RegisterOwnerAndAddFeaturedPetImageAsync();

        var response = await _httpClient.GetAsync("/v1/browse/images");
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        var first = doc.RootElement.GetProperty("items").EnumerateArray().First();
        first.TryGetProperty("ownerName", out _).ShouldBeTrue();
        first.TryGetProperty("petName", out _).ShouldBeTrue();
        first.TryGetProperty("petType", out _).ShouldBeTrue();
        first.TryGetProperty("blobName", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task GetImages_When_PageAndPageSizeProvided_Returns_Ok_WithCorrectPagination()
    {
        var response = await _httpClient.GetAsync("/v1/browse/images?page=1&pageSize=3");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        doc.RootElement.GetProperty("page").GetInt32().ShouldBe(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().ShouldBe(3);
    }

    // -----------------------------------------------------------------------
    // GET /v1/browse/pet-types
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetPetTypes_When_Called_Returns_Ok()
    {
        var response = await _httpClient.GetAsync("/v1/browse/pet-types");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPetTypes_When_Called_Returns_AllPetTypes()
    {
        var response = await _httpClient.GetAsync("/v1/browse/pet-types");

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        var types = doc.RootElement.EnumerateArray().Select(e => e.GetString()).ToList();
        types.ShouldContain("Dog");
        types.ShouldContain("Cat");
    }

    [Fact]
    public async Task GetPetTypes_When_Unauthenticated_Returns_Ok()
    {
        var response = await _httpClient.GetAsync("/v1/browse/pet-types");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // -----------------------------------------------------------------------
    // GET /v1/browse/breeds
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetBreeds_When_PetTypeIsDog_Returns_Ok_WithDogBreeds()
    {
        var response = await _httpClient.GetAsync("/v1/browse/breeds?petType=Dog");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        var breeds = doc.RootElement.EnumerateArray().Select(e => e.GetString()).ToList();
        breeds.ShouldContain("Beagle");
        breeds.ShouldContain("Golden Retriever");
        breeds.ShouldNotContain("Siamese");
    }

    [Fact]
    public async Task GetBreeds_When_PetTypeIsCat_Returns_Ok_WithCatBreeds()
    {
        var response = await _httpClient.GetAsync("/v1/browse/breeds?petType=Cat");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        var breeds = doc.RootElement.EnumerateArray().Select(e => e.GetString()).ToList();
        breeds.ShouldContain("Siamese");
        breeds.ShouldContain("Maine Coon");
        breeds.ShouldNotContain("Beagle");
    }

    [Fact]
    public async Task GetBreeds_When_PetTypeIsUnrecognised_Returns_Ok_WithEmptyArray()
    {
        var response = await _httpClient.GetAsync("/v1/browse/breeds?petType=Dragon");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        doc.RootElement.GetArrayLength().ShouldBe(0);
    }

    [Fact]
    public async Task GetBreeds_When_Unauthenticated_Returns_Ok()
    {
        var response = await _httpClient.GetAsync("/v1/browse/breeds?petType=Dog");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
