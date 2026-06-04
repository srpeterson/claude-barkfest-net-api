using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Pets.Commands.SetFeaturedImage;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class SetFeaturedImageCommandHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly SetFeaturedImageCommandHandler _setFeaturedImageCommandHandler;

    public SetFeaturedImageCommandHandlerTests()
    {
        _setFeaturedImageCommandHandler = new SetFeaturedImageCommandHandler(
            _petRepository, _unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_When_ImageExists_Sets_FeaturedAndSaves()
    {
        var pet = new PetBuilder().Build();
        var image = new PetImageBuilder().Build();
        pet.AddImage(image);
        _currentUserService.OwnerId.Returns((Guid?)pet.OwnerId);
        _petRepository.GetByIdAsync(pet.Id, CancellationToken.None).Returns(pet);

        await _setFeaturedImageCommandHandler.Handle(
            new SetFeaturedImageCommand(pet.Id, image.Id), CancellationToken.None);

        image.IsFeaturedImage.ShouldBeTrue();
        await _petRepository.Received(1).UpdateAsync(pet, CancellationToken.None);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_PetNotFound_Throws_NotFoundException()
    {
        _petRepository.GetByIdAsync(Arg.Any<Guid>(), CancellationToken.None).Returns((Pet?)null);

        await Should.ThrowAsync<NotFoundException>(() =>
            _setFeaturedImageCommandHandler.Handle(
                new SetFeaturedImageCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_PetBelongsToAnotherOwner_Throws_ForbiddenException()
    {
        var pet = new PetBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)Guid.NewGuid());
        _petRepository.GetByIdAsync(pet.Id, CancellationToken.None).Returns(pet);

        await Should.ThrowAsync<ForbiddenException>(() =>
            _setFeaturedImageCommandHandler.Handle(
                new SetFeaturedImageCommand(pet.Id, Guid.NewGuid()), CancellationToken.None));
    }
}
