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
    public async Task Handle_When_OwnerHasPets_Returns_PetsMappedToDtos()
    {
        var ownerId = Guid.NewGuid();
        var pets = new[]
        {
            new PetBuilder().WithOwnerId(ownerId).WithName("Max").Build(),
            new PetBuilder().WithOwnerId(ownerId).WithName("Daisy").Build()
        };
        _petRepository.GetByOwnerIdAsync(ownerId, CancellationToken.None).Returns(pets);

        var result = await _sut.Handle(new GetPetsByOwnerIdQuery(ownerId), CancellationToken.None);

        var list = result.ToList();
        list.Count.ShouldBe(2);
        list.ShouldAllBe(p => p.OwnerId == ownerId);
    }

    [Fact]
    public async Task Handle_When_OwnerHasNoPets_Returns_EmptyCollection()
    {
        var ownerId = Guid.NewGuid();
        _petRepository.GetByOwnerIdAsync(ownerId, CancellationToken.None).Returns([]);

        var result = await _sut.Handle(new GetPetsByOwnerIdQuery(ownerId), CancellationToken.None);

        result.ShouldBeEmpty();
    }

}
