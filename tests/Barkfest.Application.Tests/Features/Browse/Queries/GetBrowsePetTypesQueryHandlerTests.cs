using Barkfest.Application.Features.Browse.Queries.GetBrowsePetTypes;

namespace Barkfest.Application.Tests.Features.Browse.Queries;

public class GetBrowsePetTypesQueryHandlerTests
{
    private readonly GetBrowsePetTypesQueryHandler _getBrowsePetTypesQueryHandler = new();

    [Fact]
    public async Task Handle_When_Called_Returns_AllPetTypes()
    {
        var result = await _getBrowsePetTypesQueryHandler.Handle(
            new GetBrowsePetTypesQuery(), CancellationToken.None);

        result.ShouldNotBeEmpty();
        result.ShouldContain("Dog");
        result.ShouldContain("Cat");
    }

    [Fact]
    public async Task Handle_When_Called_Returns_NoDuplicates()
    {
        var result = await _getBrowsePetTypesQueryHandler.Handle(
            new GetBrowsePetTypesQuery(), CancellationToken.None);

        result.Count.ShouldBe(result.Distinct().Count());
    }

    [Fact]
    public async Task Handle_When_Called_Returns_PetTypes_OrderedByValue()
    {
        var result = await _getBrowsePetTypesQueryHandler.Handle(
            new GetBrowsePetTypesQuery(), CancellationToken.None);

        // Dog = 1, Cat = 2 — Dog should come first
        result[0].ShouldBe("Dog");
        result[1].ShouldBe("Cat");
    }
}
