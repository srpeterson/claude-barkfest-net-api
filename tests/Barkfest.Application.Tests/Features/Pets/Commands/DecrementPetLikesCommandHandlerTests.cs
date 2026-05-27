using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Features.Pets.Commands.DecrementPetLikes;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class DecrementPetLikesCommandHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DecrementPetLikesCommandHandler _decrementPetLikesCommandHandler;

    public DecrementPetLikesCommandHandlerTests()
    {
        _decrementPetLikesCommandHandler = new DecrementPetLikesCommandHandler(
            _petRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_When_LikesIsGreaterThanZero_Returns_DecrementedLikes()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        pet.IncrementLikes();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        var result = await _decrementPetLikesCommandHandler.Handle(
            new DecrementPetLikesCommand(petId), CancellationToken.None);

        result.ShouldBe(0);
        await _petRepository.Received(1).UpdateAsync(pet, CancellationToken.None);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_LikesIsZero_Returns_Zero()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        var result = await _decrementPetLikesCommandHandler.Handle(
            new DecrementPetLikesCommand(petId), CancellationToken.None);

        result.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_When_PetNotFound_Throws_NotFoundException()
    {
        var petId = Guid.NewGuid();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns((Pet?)null);

        await Should.ThrowAsync<NotFoundException>(
            () => _decrementPetLikesCommandHandler.Handle(
                new DecrementPetLikesCommand(petId), CancellationToken.None));
    }
}
