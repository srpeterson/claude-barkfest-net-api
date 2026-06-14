using Barkfest.Application.Features.Pets.Commands.IncrementPetLikes;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class IncrementPetLikesCommandHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IncrementPetLikesCommandHandler _incrementPetLikesCommandHandler;

    public IncrementPetLikesCommandHandlerTests()
    {
        _incrementPetLikesCommandHandler = new IncrementPetLikesCommandHandler(_petRepository);
    }

    [Fact]
    public async Task Handle_When_PetExists_Returns_IncrementedLikes()
    {
        var petId = Guid.NewGuid();
        _petRepository.IncrementLikesAsync(petId, CancellationToken.None)
            .Returns(new LikeUpdateResult(PetExists: true, Likes: 5));

        var result = await _incrementPetLikesCommandHandler.Handle(
            new IncrementPetLikesCommand(petId), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(5);
        await _petRepository.Received(1).IncrementLikesAsync(petId, CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_PetNotFound_Throws_NotFoundException()
    {
        var petId = Guid.NewGuid();
        _petRepository.IncrementLikesAsync(petId, CancellationToken.None)
            .Returns(new LikeUpdateResult(PetExists: false, Likes: 0));

        var result = await _incrementPetLikesCommandHandler.Handle(
            new IncrementPetLikesCommand(petId), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<NotFoundError>();
    }
}
