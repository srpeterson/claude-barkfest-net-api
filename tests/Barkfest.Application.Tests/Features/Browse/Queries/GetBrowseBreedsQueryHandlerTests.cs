using Barkfest.Application.Features.Browse.Queries.GetBrowseBreeds;

namespace Barkfest.Application.Tests.Features.Browse.Queries;

public class GetBrowseBreedsQueryHandlerTests
{
    private readonly GetBrowseBreedsQueryHandler _getBrowseBreedsQueryHandler = new();

    // -----------------------------------------------------------------------
    // Dog breeds
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_PetTypeIsDog_Returns_DogBreeds()
    {
        var result = await _getBrowseBreedsQueryHandler.Handle(
            new GetBrowseBreedsQuery("Dog"), CancellationToken.None);

        result.ShouldNotBeEmpty();
        result.ShouldContain("Beagle");
        result.ShouldContain("Golden Retriever");
        result.ShouldContain("French Bulldog");
    }

    [Fact]
    public async Task Handle_When_PetTypeIsDog_DoesNotReturn_CatBreeds()
    {
        var result = await _getBrowseBreedsQueryHandler.Handle(
            new GetBrowseBreedsQuery("Dog"), CancellationToken.None);

        result.ShouldNotContain("Siamese");
        result.ShouldNotContain("Maine Coon");
    }

    [Fact]
    public async Task Handle_When_PetTypeIsDog_IsCaseInsensitive()
    {
        var result = await _getBrowseBreedsQueryHandler.Handle(
            new GetBrowseBreedsQuery("dog"), CancellationToken.None);

        result.ShouldNotBeEmpty();
        result.ShouldContain("Beagle");
    }

    // -----------------------------------------------------------------------
    // Cat breeds
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_PetTypeIsCat_Returns_CatBreeds()
    {
        var result = await _getBrowseBreedsQueryHandler.Handle(
            new GetBrowseBreedsQuery("Cat"), CancellationToken.None);

        result.ShouldNotBeEmpty();
        result.ShouldContain("Siamese");
        result.ShouldContain("Maine Coon");
        result.ShouldContain("Bengal");
    }

    [Fact]
    public async Task Handle_When_PetTypeIsCat_DoesNotReturn_DogBreeds()
    {
        var result = await _getBrowseBreedsQueryHandler.Handle(
            new GetBrowseBreedsQuery("Cat"), CancellationToken.None);

        result.ShouldNotContain("Beagle");
        result.ShouldNotContain("Labrador Retriever");
    }

    [Fact]
    public async Task Handle_When_PetTypeIsCat_IsCaseInsensitive()
    {
        var result = await _getBrowseBreedsQueryHandler.Handle(
            new GetBrowseBreedsQuery("cat"), CancellationToken.None);

        result.ShouldNotBeEmpty();
        result.ShouldContain("Maine Coon");
    }

    // -----------------------------------------------------------------------
    // Invalid / unrecognised pet type
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_PetTypeIsUnrecognised_Returns_EmptyList()
    {
        var result = await _getBrowseBreedsQueryHandler.Handle(
            new GetBrowseBreedsQuery("Unicorn"), CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_When_PetTypeIsEmpty_Returns_EmptyList()
    {
        var result = await _getBrowseBreedsQueryHandler.Handle(
            new GetBrowseBreedsQuery(""), CancellationToken.None);

        result.ShouldBeEmpty();
    }
}
