using Barkfest.Infrastructure.Storage;
using System.Text;

namespace Barkfest.Infrastructure.Tests.Storage;

public class AzureBlobStorageServiceTests(AzuriteFixture fixture)
    : IClassFixture<AzuriteFixture>
{
    // Each test uses a unique container name so tests are fully isolated
    // from one another without needing transaction rollback.
    private static string UniqueContainer() => $"test-{Guid.NewGuid():N}";

    private AzureBlobStorageService CreateSut() =>
        new AzureBlobStorageService(fixture.CreateBlobServiceClient());

    // -----------------------------------------------------------------------
    // UploadAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task UploadAsync_When_ContainerDoesNotExist_Creates_Container()
    {
        var sut = CreateSut();
        var container = UniqueContainer();

        await sut.UploadAsync(container, "auto-container.txt", TextStream("hello"), "text/plain");

        var exists = await sut.ExistsAsync(container, "auto-container.txt");
        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task UploadAsync_When_BlobAlreadyExists_Overwrites_Blob()
    {
        var sut = CreateSut();
        var container = UniqueContainer();
        const string blobName = "overwrite-me.txt";

        await sut.UploadAsync(container, blobName, TextStream("original"), "text/plain");
        await sut.UploadAsync(container, blobName, TextStream("updated"), "text/plain");

        using var stream = await sut.DownloadAsync(container, blobName);
        var content = await new StreamReader(stream).ReadToEndAsync();
        content.ShouldBe("updated");
    }

    // -----------------------------------------------------------------------
    // DownloadAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DownloadAsync_When_BlobExists_Returns_OriginalContent()
    {
        var sut = CreateSut();
        var container = UniqueContainer();
        const string blobName = "download-me.txt";
        const string payload = "Barkfest blob content";

        await sut.UploadAsync(container, blobName, TextStream(payload), "text/plain");

        using var stream = await sut.DownloadAsync(container, blobName);
        var result = await new StreamReader(stream).ReadToEndAsync();

        result.ShouldBe(payload);
    }

    [Fact]
    public async Task DownloadAsync_When_BlobUploaded_Preserves_ContentType()
    {
        var sut = CreateSut();
        var container = UniqueContainer();
        const string blobName = "image.png";

        await sut.UploadAsync(container, blobName, TextStream("fake-png"), "image/png");

        // Verify via the blob client directly that the content type was stored
        var blobClient = fixture.CreateBlobServiceClient()
            .GetBlobContainerClient(container)
            .GetBlobClient(blobName);
        var props = await blobClient.GetPropertiesAsync();

        props.Value.ContentType.ShouldBe("image/png");
    }

    // -----------------------------------------------------------------------
    // ExistsAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ExistsAsync_When_BlobExists_Returns_True()
    {
        var sut = CreateSut();
        var container = UniqueContainer();

        await sut.UploadAsync(container, "exists.txt", TextStream("data"), "text/plain");

        var result = await sut.ExistsAsync(container, "exists.txt");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsAsync_When_BlobDoesNotExist_Returns_False()
    {
        var sut = CreateSut();

        var result = await sut.ExistsAsync(UniqueContainer(), "no-such-blob.txt");

        result.ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // DeleteAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_When_BlobExists_Removes_Blob()
    {
        var sut = CreateSut();
        var container = UniqueContainer();
        const string blobName = "delete-me.txt";

        await sut.UploadAsync(container, blobName, TextStream("bye"), "text/plain");
        await sut.DeleteAsync(container, blobName);

        var exists = await sut.ExistsAsync(container, blobName);
        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteAsync_When_BlobDoesNotExist_Succeeds()
    {
        var sut = CreateSut();

        // DeleteIfExistsAsync — should be a no-op, not an exception
        await Should.NotThrowAsync(() =>
            sut.DeleteAsync(UniqueContainer(), "ghost.txt"));
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static Stream TextStream(string text) =>
        new MemoryStream(Encoding.UTF8.GetBytes(text));
}
