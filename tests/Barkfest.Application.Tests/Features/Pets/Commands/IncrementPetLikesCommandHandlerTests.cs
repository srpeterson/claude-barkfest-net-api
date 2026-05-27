using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Features.Pets.Commands.IncrementPetLikes;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class IncrementPetLikesCommandHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IncrementPetLikesCommandHandler _incrementPetLikesCommandHandler;

    public IncrementPetLikesCommandHandlerTests()
    {
        _incrementPetLikesCommandHandler = new IncrementPetLikesCommandHandler(
            _petRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_When_PetExists_Returns_IncrementedLikes()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        var result = await _incrementPetLikesCommandHandler.Handle(
            new IncrementPetLikesCommand(petId), CancellationToken.None);

        result.ShouldBe(1);
        await _petRepository.Received(1).UpdateAsync(pet, CancellationToken.None);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_PetNotFound_Throws_NotFoundException()
    {
        var petId = Guid.NewGuid();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns((Pet?)null);

        await Should.ThrowAsync<NotFoundException>(
            () => _incrementPetLikesCommandHandler.Handle(
                new IncrementPetLikesCommand(petId), CancellationToken.None));
    }
}
