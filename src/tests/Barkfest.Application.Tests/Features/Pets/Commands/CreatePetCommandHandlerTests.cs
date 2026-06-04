using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Pets.Commands.CreatePet;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class CreatePetCommandHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly CreatePetCommandHandler _createPetCommandHandler;

    public CreatePetCommandHandlerTests()
    {
        _createPetCommandHandler = new CreatePetCommandHandler(_ownerRepository, _petRepository, _unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_When_CommandIsValid_Returns_ValidGuid()
    {
        var owner = new OwnerBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)owner.Id);
        _ownerRepository.GetByIdAsync(owner.Id, CancellationToken.None).Returns(owner);

        var command = new CreatePetCommand("Buddy", null, null, PetType.Dog.Value, DogBreed.Beagle.Value);

        var result = await _createPetCommandHandler.Handle(command, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_When_CommandIsValid_Adds_PetAndSaves()
    {
        var owner = new OwnerBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)owner.Id);
        _ownerRepository.GetByIdAsync(owner.Id, CancellationToken.None).Returns(owner);

        var dob = new DateOnly(2020, 6, 15);
        var command = new CreatePetCommand("Max", "A good boy", dob, PetType.Dog.Value, DogBreed.Beagle.Value);

        await _createPetCommandHandler.Handle(command, CancellationToken.None);

        await _petRepository.Received(1).AddAsync(
            Arg.Is<Pet>(p =>
                p.Name == "Max" &&
                p.Description == "A good boy" &&
                p.DateOfBirth == dob &&
                p.OwnerId == owner.Id),
            CancellationToken.None);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_OwnerNotFound_Throws_NotFoundException()
    {
        var ownerId = Guid.NewGuid();
        _currentUserService.OwnerId.Returns((Guid?)ownerId);
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns((Owner?)null);

        var command = new CreatePetCommand("Buddy", null, null, PetType.Dog.Value, DogBreed.Beagle.Value);

        await Should.ThrowAsync<NotFoundException>(() => _createPetCommandHandler.Handle(command, CancellationToken.None));
    }
}
