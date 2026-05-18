using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Features.Pets.Queries.GetPetById;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Queries;

public class GetPetByIdQueryHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly GetPetByIdQueryHandler _sut;

    public GetPetByIdQueryHandlerTests()
    {
        _sut = new GetPetByIdQueryHandler(_petRepository);
    }

    [Fact]
    public async Task Handle_When_PetExists_Returns_MappedDto()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        var result = await _sut.Handle(new GetPetByIdQuery(petId), CancellationToken.None);

        result.Name.ShouldBe("Buddy");
        result.PetType.ShouldBe("Dog");
    }

    [Fact]
    public async Task Handle_When_PetNotFound_Throws_NotFoundException()
    {
        var petId = Guid.NewGuid();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns((Pet?)null);

        await Should.ThrowAsync<NotFoundException>(
            () => _sut.Handle(new GetPetByIdQuery(petId), CancellationToken.None));
    }

}
