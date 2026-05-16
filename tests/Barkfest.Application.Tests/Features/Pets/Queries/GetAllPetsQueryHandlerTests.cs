using Barkfest.Application.Features.Pets.Queries.GetAllPets;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Queries;

public class GetAllPetsQueryHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly GetAllPetsQueryHandler _sut;

    public GetAllPetsQueryHandlerTests()
    {
        _sut = new GetAllPetsQueryHandler(_petRepository);
    }

    [Fact]
    public async Task Handle_ReturnAllPetsMappedToDtos()
    {
        var pets = new[] { BuildPet("Buddy"), BuildPet("Luna") };
        _petRepository.GetAllAsync(CancellationToken.None).Returns(pets);

        var result = await _sut.Handle(new GetAllPetsQuery(), CancellationToken.None);

        var list = result.ToList();
        list.Count.ShouldBe(2);
        list[0].Name.ShouldBe("Buddy");
        list[1].Name.ShouldBe("Luna");
    }

    [Fact]
    public async Task Handle_NoPets_ReturnsEmptyCollection()
    {
        _petRepository.GetAllAsync(CancellationToken.None).Returns([]);

        var result = await _sut.Handle(new GetAllPetsQuery(), CancellationToken.None);

        result.ShouldBeEmpty();
    }

    private static Pet BuildPet(string name)
    {
        var pet = new Pet(Guid.NewGuid());
        pet.SetName(name);
        pet.SetPetType(Barkfest.Domain.Enums.PetType.Dog);
        return pet;
    }
}
