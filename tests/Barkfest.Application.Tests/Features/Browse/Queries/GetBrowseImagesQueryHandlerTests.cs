using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Browse.DTOs;
using Barkfest.Application.Features.Browse.Queries;
using Barkfest.Domain.Enums;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Browse.Queries;

public class GetBrowseImagesQueryHandlerTests
{
    private readonly IBrowseRepository _browseRepository = Substitute.For<IBrowseRepository>();
    private readonly GetBrowseImagesQueryHandler _getBrowseImagesQueryHandler;

    public GetBrowseImagesQueryHandlerTests()
    {
        _getBrowseImagesQueryHandler = new GetBrowseImagesQueryHandler(_browseRepository);
    }

    // -----------------------------------------------------------------------
    // No filters
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_NoFilters_Returns_AllImages()
    {
        var images = new[]
        {
            new BrowseImageDto(Guid.NewGuid(), "pets/1/img.jpg", "image/jpeg", false, DateTime.UtcNow,
                "Alice Adams", Guid.NewGuid(), "Buddy", null, null, null, "Dog", null),
            new BrowseImageDto(Guid.NewGuid(), "pets/2/img.jpg", "image/jpeg", false, DateTime.UtcNow,
                "Bob Baker", Guid.NewGuid(), "Whiskers", null, null, null, "Cat", null)
        };
        _browseRepository.GetBrowseImagesAsync(null, null, CancellationToken.None).Returns(images);

        var result = await _getBrowseImagesQueryHandler.Handle(
            new GetBrowseImagesQuery(null, null), CancellationToken.None);

        result.Count().ShouldBe(2);
    }

    // -----------------------------------------------------------------------
    // PetType filter
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_PetTypeIsValid_Passes_ParsedPetType_ToRepository()
    {
        _browseRepository.GetBrowseImagesAsync(Arg.Any<PetType?>(), null, CancellationToken.None).Returns([]);

        await _getBrowseImagesQueryHandler.Handle(
            new GetBrowseImagesQuery("Dog", null), CancellationToken.None);

        await _browseRepository.Received(1).GetBrowseImagesAsync(
            Arg.Is<PetType?>(pt => pt != null && pt.Name == "Dog"),
            null,
            CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_PetTypeIsUnrecognised_Returns_EmptyCollection_WithoutCallingRepository()
    {
        var result = await _getBrowseImagesQueryHandler.Handle(
            new GetBrowseImagesQuery("Unicorn", null), CancellationToken.None);

        result.ShouldBeEmpty();
        await _browseRepository.DidNotReceive().GetBrowseImagesAsync(
            Arg.Any<PetType?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    // -----------------------------------------------------------------------
    // Breed filter
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_BreedProvided_Passes_BreedString_ToRepository()
    {
        _browseRepository.GetBrowseImagesAsync(Arg.Any<PetType?>(), "LabradorRetriever", CancellationToken.None).Returns([]);

        await _getBrowseImagesQueryHandler.Handle(
            new GetBrowseImagesQuery("Dog", "LabradorRetriever"), CancellationToken.None);

        await _browseRepository.Received(1).GetBrowseImagesAsync(
            Arg.Is<PetType?>(pt => pt != null && pt.Name == "Dog"),
            "LabradorRetriever",
            CancellationToken.None);
    }
}
