using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Features.Pets.Commands.DeletePet;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class DeletePetCommandHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeletePetCommandHandler _sut;

    public DeletePetCommandHandlerTests()
    {
        _sut = new DeletePetCommandHandler(_petRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ExistingPet_DeletesAndSaves()
    {
        var petId = Guid.NewGuid();
        var pet = BuildPet();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        await _sut.Handle(new DeletePetCommand(petId), CancellationToken.None);

        await _petRepository.Received(1).DeleteAsync(petId, CancellationToken.None);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_PetNotFound_ThrowsNotFoundException()
    {
        var petId = Guid.NewGuid();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns((Pet?)null);

        await Should.ThrowAsync<NotFoundException>(
            () => _sut.Handle(new DeletePetCommand(petId), CancellationToken.None));
    }

    private static Pet BuildPet()
    {
        var pet = new Pet(Guid.NewGuid());
        pet.SetName("Buddy");
        pet.SetPetType(Barkfest.Domain.Enums.PetType.Dog);
        return pet;
    }
}
