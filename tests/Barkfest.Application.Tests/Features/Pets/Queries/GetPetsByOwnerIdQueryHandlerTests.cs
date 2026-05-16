using Barkfest.Application.Features.Pets.Queries.GetPetsByOwnerId;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Queries;

public class GetPetsByOwnerIdQueryHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly GetPetsByOwnerIdQueryHandler _sut;

    public GetPetsByOwnerIdQueryHandlerTests()
    {
        _sut = new GetPetsByOwnerIdQueryHandler(_petRepository);
    }

    [Fact]
    public async Task Handle_ReturnsPetsForOwnerMappedToDtos()
    {
        var ownerId = Guid.NewGuid();
        var pets = new[] { BuildPet(ownerId, "Max"), BuildPet(ownerId, "Daisy") };
        _petRepository.GetByOwnerIdAsync(ownerId, CancellationToken.None).Returns(pets);

        var result = await _sut.Handle(new GetPetsByOwnerIdQuery(ownerId), CancellationToken.None);

        var list = result.ToList();
        list.Count.ShouldBe(2);
        list.ShouldAllBe(p => p.OwnerId == ownerId);
    }

    [Fact]
    public async Task Handle_OwnerHasNoPets_ReturnsEmptyCollection()
    {
        var ownerId = Guid.NewGuid();
        _petRepository.GetByOwnerIdAsync(ownerId, CancellationToken.None).Returns([]);

        var result = await _sut.Handle(new GetPetsByOwnerIdQuery(ownerId), CancellationToken.None);

        result.ShouldBeEmpty();
    }

    private static Pet BuildPet(Guid ownerId, string name)
    {
        var pet = new Pet(ownerId);
        pet.SetName(name);
        pet.SetPetType(Barkfest.Domain.Enums.PetType.Dog);
        return pet;
    }
}
