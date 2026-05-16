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
    public async Task Handle_ExistingPet_ReturnsMappedDto()
    {
        var petId = Guid.NewGuid();
        var pet = BuildPet();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        var result = await _sut.Handle(new GetPetByIdQuery(petId), CancellationToken.None);

        result.Name.ShouldBe("Buddy");
        result.PetType.ShouldBe("Dog");
    }

    [Fact]
    public async Task Handle_PetNotFound_ThrowsNotFoundException()
    {
        var petId = Guid.NewGuid();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns((Pet?)null);

        await Should.ThrowAsync<NotFoundException>(
            () => _sut.Handle(new GetPetByIdQuery(petId), CancellationToken.None));
    }

    private static Pet BuildPet()
    {
        var pet = new Pet(Guid.NewGuid());
        pet.SetName("Buddy");
        pet.SetPetType(Barkfest.Domain.Enums.PetType.Dog);
        return pet;
    }
}
