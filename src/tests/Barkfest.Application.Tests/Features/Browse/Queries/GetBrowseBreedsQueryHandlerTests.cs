using Barkfest.Application.Features.Browse.Queries.GetBrowseBreeds;
using Barkfest.Domain.Enums;

namespace Barkfest.Application.Tests.Features.Browse.Queries;

public class GetBrowseBreedsQueryHandlerTests
{
    private readonly GetBrowseBreedsQueryHandler _getBrowseBreedsQueryHandler = new();

    // -----------------------------------------------------------------------
    // Dog breeds
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_PetTypeValueIsDog_Returns_DogBreeds()
    {
        var result = await _getBrowseBreedsQueryHandler.Handle(
            new GetBrowseBreedsQuery(PetType.Dog.Value), CancellationToken.None);

        result.ShouldNotBeEmpty();
        result.ShouldContain(b => b.Name == "Beagle");
        result.ShouldContain(b => b.Name == "Golden Retriever");
        result.ShouldContain(b => b.Name == "French Bulldog");
    }

    [Fact]
    public async Task Handle_When_PetTypeValueIsDog_DoesNotReturn_CatBreeds()
    {
        var result = await _getBrowseBreedsQueryHandler.Handle(
            new GetBrowseBreedsQuery(PetType.Dog.Value), CancellationToken.None);

        result.ShouldNotContain(b => b.Name == "Siamese");
        result.ShouldNotContain(b => b.Name == "Maine Coon");
    }

    // -----------------------------------------------------------------------
    // Cat breeds
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_PetTypeValueIsCat_Returns_CatBreeds()
    {
        var result = await _getBrowseBreedsQueryHandler.Handle(
            new GetBrowseBreedsQuery(PetType.Cat.Value), CancellationToken.None);

        result.ShouldNotBeEmpty();
        result.ShouldContain(b => b.Name == "Siamese");
        result.ShouldContain(b => b.Name == "Maine Coon");
        result.ShouldContain(b => b.Name == "Bengal");
    }

    [Fact]
    public async Task Handle_When_PetTypeValueIsCat_DoesNotReturn_DogBreeds()
    {
        var result = await _getBrowseBreedsQueryHandler.Handle(
            new GetBrowseBreedsQuery(PetType.Cat.Value), CancellationToken.None);

        result.ShouldNotContain(b => b.Name == "Beagle");
        result.ShouldNotContain(b => b.Name == "Labrador Retriever");
    }

    // -----------------------------------------------------------------------
    // Invalid / unrecognised pet type value
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_PetTypeValueIsUnrecognised_Returns_EmptyList()
    {
        var result = await _getBrowseBreedsQueryHandler.Handle(
            new GetBrowseBreedsQuery(99), CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_When_PetTypeValueIsZero_Returns_EmptyList()
    {
        var result = await _getBrowseBreedsQueryHandler.Handle(
            new GetBrowseBreedsQuery(0), CancellationToken.None);

        result.ShouldBeEmpty();
    }
}
