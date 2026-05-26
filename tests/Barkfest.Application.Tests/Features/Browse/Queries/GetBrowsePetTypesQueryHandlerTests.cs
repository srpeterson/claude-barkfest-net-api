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
        result.ShouldContain(pt => pt.Name == "Dog" && pt.Value == 1);
        result.ShouldContain(pt => pt.Name == "Cat" && pt.Value == 2);
    }

    [Fact]
    public async Task Handle_When_Called_Returns_NoDuplicates()
    {
        var result = await _getBrowsePetTypesQueryHandler.Handle(
            new GetBrowsePetTypesQuery(), CancellationToken.None);

        result.Count.ShouldBe(result.Select(pt => pt.Value).Distinct().Count());
    }

    [Fact]
    public async Task Handle_When_Called_Returns_PetTypes_OrderedByValue()
    {
        var result = await _getBrowsePetTypesQueryHandler.Handle(
            new GetBrowsePetTypesQuery(), CancellationToken.None);

        // Dog = 1, Cat = 2 — Dog should come first
        result[0].Name.ShouldBe("Dog");
        result[0].Value.ShouldBe(1);
        result[1].Name.ShouldBe("Cat");
        result[1].Value.ShouldBe(2);
    }
}
