using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Common.Models;
using Barkfest.Application.Features.Browse.DTOs;
using Barkfest.Application.Features.Browse.Queries;
using Barkfest.Domain.Enums;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Browse.Queries;

public class GetBrowseImagesQueryHandlerTests
{
    private readonly IBrowseRepository _browseRepository = Substitute.For<IBrowseRepository>();
    private readonly GetBrowseImagesQueryHandler _getBrowseImagesQueryHandler;

    private const int DefaultPage     = 1;
    private const int DefaultPageSize = 6;

    public GetBrowseImagesQueryHandlerTests()
    {
        _getBrowseImagesQueryHandler = new GetBrowseImagesQueryHandler(_browseRepository);
    }

    // -----------------------------------------------------------------------
    // No filters
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_NoFilters_Returns_PagedResult_WithItems()
    {
        var images = new[]
        {
            new BrowseImageDto(Guid.NewGuid(), "pets/1/img.jpg", "image/jpeg", true, DateTime.UtcNow,
                "Alice Adams", Guid.NewGuid(), "Buddy", null, null, null, "Dog", null),
            new BrowseImageDto(Guid.NewGuid(), "pets/2/img.jpg", "image/jpeg", true, DateTime.UtcNow,
                "Bob Baker", Guid.NewGuid(), "Whiskers", null, null, null, "Cat", null)
        };
        var pagedResult = new PagedResult<BrowseImageDto>(images, DefaultPage, DefaultPageSize, 2);

        _browseRepository
            .GetBrowseImagesAsync(null, null, DefaultPage, DefaultPageSize, CancellationToken.None)
            .Returns(pagedResult);

        var result = await _getBrowseImagesQueryHandler.Handle(
            new GetBrowseImagesQuery(null, null, DefaultPage, DefaultPageSize), CancellationToken.None);

        result.Items.Count.ShouldBe(2);
        result.TotalCount.ShouldBe(2);
        result.Page.ShouldBe(DefaultPage);
        result.PageSize.ShouldBe(DefaultPageSize);
    }

    // -----------------------------------------------------------------------
    // PetType filter
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_PetTypeValueIsValid_Passes_ResolvedPetType_ToRepository()
    {
        _browseRepository
            .GetBrowseImagesAsync(
                Arg.Any<PetType?>(), null, DefaultPage, DefaultPageSize, CancellationToken.None)
            .Returns(new PagedResult<BrowseImageDto>([], DefaultPage, DefaultPageSize, 0));

        await _getBrowseImagesQueryHandler.Handle(
            new GetBrowseImagesQuery(PetType.Dog.Value, null, DefaultPage, DefaultPageSize), CancellationToken.None);

        await _browseRepository.Received(1).GetBrowseImagesAsync(
            Arg.Is<PetType?>(pt => pt != null && pt == PetType.Dog),
            null,
            DefaultPage,
            DefaultPageSize,
            CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_PetTypeValueIsUnrecognised_Returns_EmptyPagedResult_WithoutCallingRepository()
    {
        var result = await _getBrowseImagesQueryHandler.Handle(
            new GetBrowseImagesQuery(99, null, DefaultPage, DefaultPageSize), CancellationToken.None);

        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
        await _browseRepository.DidNotReceive().GetBrowseImagesAsync(
            Arg.Any<PetType?>(), Arg.Any<int?>(),
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    // -----------------------------------------------------------------------
    // Breed filter
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_BreedValueProvided_Passes_BreedValue_ToRepository()
    {
        _browseRepository
            .GetBrowseImagesAsync(
                Arg.Any<PetType?>(), DogBreed.LabradorRetriever.Value, DefaultPage, DefaultPageSize, CancellationToken.None)
            .Returns(new PagedResult<BrowseImageDto>([], DefaultPage, DefaultPageSize, 0));

        await _getBrowseImagesQueryHandler.Handle(
            new GetBrowseImagesQuery(PetType.Dog.Value, DogBreed.LabradorRetriever.Value, DefaultPage, DefaultPageSize), CancellationToken.None);

        await _browseRepository.Received(1).GetBrowseImagesAsync(
            Arg.Is<PetType?>(pt => pt != null && pt == PetType.Dog),
            DogBreed.LabradorRetriever.Value,
            DefaultPage,
            DefaultPageSize,
            CancellationToken.None);
    }

    // -----------------------------------------------------------------------
    // Pagination passthrough
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_PageAndPageSizeProvided_Passes_Values_ToRepository()
    {
        _browseRepository
            .GetBrowseImagesAsync(null, null, 2, 12, CancellationToken.None)
            .Returns(new PagedResult<BrowseImageDto>([], 2, 12, 0));

        await _getBrowseImagesQueryHandler.Handle(
            new GetBrowseImagesQuery(null, null, 2, 12), CancellationToken.None);

        await _browseRepository.Received(1).GetBrowseImagesAsync(
            null, null, 2, 12, CancellationToken.None);
    }
}
