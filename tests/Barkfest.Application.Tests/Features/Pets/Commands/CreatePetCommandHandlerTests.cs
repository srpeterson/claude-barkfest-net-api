using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Features.Pets.Commands.CreatePet;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class CreatePetCommandHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreatePetCommandHandler _sut;

    public CreatePetCommandHandlerTests()
    {
        _sut = new CreatePetCommandHandler(_ownerRepository, _petRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_When_CommandIsValid_Returns_ValidGuid()
    {
        var ownerId = Guid.NewGuid();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(new OwnerBuilder().Build());

        var command = new CreatePetCommand(ownerId, "Buddy", null, null, "Dog");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_When_CommandIsValid_Adds_PetAndSaves()
    {
        var ownerId = Guid.NewGuid();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(new OwnerBuilder().Build());

        var dob = new DateOnly(2020, 6, 15);
        var command = new CreatePetCommand(ownerId, "Max", "A good boy", dob, "Dog");

        await _sut.Handle(command, CancellationToken.None);

        await _petRepository.Received(1).AddAsync(
            Arg.Is<Pet>(p =>
                p.Name == "Max" &&
                p.Description == "A good boy" &&
                p.DateOfBirth == dob &&
                p.OwnerId == ownerId),
            CancellationToken.None);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_OwnerNotFound_Throws_NotFoundException()
    {
        var ownerId = Guid.NewGuid();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns((Owner?)null);

        var command = new CreatePetCommand(ownerId, "Buddy", null, null, "Dog");

        await Should.ThrowAsync<NotFoundException>(() => _sut.Handle(command, CancellationToken.None));
    }

}
