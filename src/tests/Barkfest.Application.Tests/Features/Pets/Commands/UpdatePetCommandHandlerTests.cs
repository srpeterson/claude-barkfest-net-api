using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Pets.Commands.UpdatePet;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class UpdatePetCommandHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly UpdatePetCommandHandler _updatePetCommandHandler;

    public UpdatePetCommandHandlerTests()
    {
        _updatePetCommandHandler = new UpdatePetCommandHandler(_petRepository, _unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_When_PetExists_Updates_AndSaves()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)pet.OwnerId);
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        var command = new UpdatePetCommand(petId, "Luna", "Updated desc", null, PetType.Cat.Value, CatBreed.Siamese.Value);

        await _updatePetCommandHandler.Handle(command, CancellationToken.None);

        await _petRepository.Received(1).UpdateAsync(
            Arg.Is<Pet>(p =>
                p.Name == "Luna" &&
                p.Description == "Updated desc" &&
                p.PetType == PetType.Cat &&
                p.BreedValue == CatBreed.Siamese.Value),
            CancellationToken.None);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_PetNotFound_Throws_NotFoundException()
    {
        var petId = Guid.NewGuid();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns((Pet?)null);

        var command = new UpdatePetCommand(petId, "Luna", null, null, PetType.Cat.Value, CatBreed.Siamese.Value);

        await Should.ThrowAsync<NotFoundException>(() => _updatePetCommandHandler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_PetBelongsToAnotherOwner_Throws_ForbiddenException()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)Guid.NewGuid());
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        var command = new UpdatePetCommand(petId, "Luna", null, null, PetType.Cat.Value, CatBreed.Siamese.Value);

        await Should.ThrowAsync<ForbiddenException>(() => _updatePetCommandHandler.Handle(command, CancellationToken.None));
    }
}
