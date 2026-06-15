using Barkfest.Application.Features.Pets.Commands.DecrementPetLikes;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class DecrementPetLikesCommandHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly DecrementPetLikesCommandHandler _decrementPetLikesCommandHandler;

    public DecrementPetLikesCommandHandlerTests()
    {
        _decrementPetLikesCommandHandler = new DecrementPetLikesCommandHandler(_petRepository);
    }

    [Fact]
    public async Task Handle_When_PetExists_Returns_DecrementedLikes()
    {
        var petId = Guid.NewGuid();
        _petRepository.DecrementLikesAsync(petId, CancellationToken.None)
            .Returns(new LikeUpdateResult(PetExists: true, Likes: 2));

        var result = await _decrementPetLikesCommandHandler.Handle(
            new DecrementPetLikesCommand(petId), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(2);
        await _petRepository.Received(1).DecrementLikesAsync(petId, CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_LikesIsZero_Returns_Zero()
    {
        var petId = Guid.NewGuid();
        _petRepository.DecrementLikesAsync(petId, CancellationToken.None)
            .Returns(new LikeUpdateResult(PetExists: true, Likes: 0));

        var result = await _decrementPetLikesCommandHandler.Handle(
            new DecrementPetLikesCommand(petId), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_When_PetNotFound_Returns_NotFoundError()
    {
        var petId = Guid.NewGuid();
        _petRepository.DecrementLikesAsync(petId, CancellationToken.None)
            .Returns(new LikeUpdateResult(PetExists: false, Likes: 0));

        var result = await _decrementPetLikesCommandHandler.Handle(
            new DecrementPetLikesCommand(petId), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<NotFoundError>();
    }
}
