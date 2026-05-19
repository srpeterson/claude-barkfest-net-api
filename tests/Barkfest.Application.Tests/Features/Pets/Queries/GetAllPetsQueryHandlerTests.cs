using Barkfest.Application.Features.Pets.Queries.GetAllPets;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Queries;

public class GetAllPetsQueryHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly GetAllPetsQueryHandler _getAllPetsQueryHandler;

    public GetAllPetsQueryHandlerTests()
    {
        _getAllPetsQueryHandler = new GetAllPetsQueryHandler(_petRepository);
    }

    [Fact]
    public async Task Handle_When_PetsExist_Returns_AllPetsMappedToDtos()
    {
        var pets = new[]
        {
            new PetBuilder().WithName("Buddy").Build(),
            new PetBuilder().WithName("Luna").Build()
        };
        _petRepository.GetAllAsync(CancellationToken.None).Returns(pets);

        var result = await _getAllPetsQueryHandler.Handle(new GetAllPetsQuery(), CancellationToken.None);

        var list = result.ToList();
        list.Count.ShouldBe(2);
        list[0].Name.ShouldBe("Buddy");
        list[1].Name.ShouldBe("Luna");
    }

    [Fact]
    public async Task Handle_When_NoPets_Returns_EmptyCollection()
    {
        _petRepository.GetAllAsync(CancellationToken.None).Returns([]);

        var result = await _getAllPetsQueryHandler.Handle(new GetAllPetsQuery(), CancellationToken.None);

        result.ShouldBeEmpty();
    }

}
