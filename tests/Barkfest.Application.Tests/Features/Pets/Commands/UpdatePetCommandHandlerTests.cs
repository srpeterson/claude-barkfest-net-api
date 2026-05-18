using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Features.Pets.Commands.UpdatePet;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class UpdatePetCommandHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdatePetCommandHandler _sut;

    public UpdatePetCommandHandlerTests()
    {
        _sut = new UpdatePetCommandHandler(_petRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_When_PetExists_Updates_AndSaves()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        var command = new UpdatePetCommand(petId, "Luna", "Updated desc", null, "Cat");

        await _sut.Handle(command, CancellationToken.None);

        await _petRepository.Received(1).UpdateAsync(
            Arg.Is<Pet>(p => p.Name == "Luna" && p.Description == "Updated desc"),
            CancellationToken.None);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_PetNotFound_Throws_NotFoundException()
    {
        var petId = Guid.NewGuid();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns((Pet?)null);

        var command = new UpdatePetCommand(petId, "Luna", null, null, "Cat");

        await Should.ThrowAsync<NotFoundException>(() => _sut.Handle(command, CancellationToken.None));
    }

}
