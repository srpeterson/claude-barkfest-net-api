using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Pets.Commands.RemovePetImage;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class RemovePetImageCommandHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IBlobStorageService _blobStorageService = Substitute.For<IBlobStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly RemovePetImageCommandHandler _removePetImageCommandHandler;

    public RemovePetImageCommandHandlerTests()
    {
        _removePetImageCommandHandler = new RemovePetImageCommandHandler(_petRepository, _blobStorageService, _unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_When_PetAndImageExist_Deletes_BlobAndRemovesFromPet()
    {
        var petId = Guid.NewGuid();
        var petImage = new PetImageBuilder().Build();
        var pet = new PetBuilder().WithImage(petImage).Build();
        _currentUserService.OwnerId.Returns((Guid?)pet.OwnerId);
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        var command = new RemovePetImageCommand(petId, petImage.Id);

        await _removePetImageCommandHandler.Handle(command, CancellationToken.None);

        await _blobStorageService.Received(1).DeleteAsync(
            "pet-images", petImage.BlobName, CancellationToken.None);
        pet.Images.ShouldBeEmpty();
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_PetNotFound_Returns_NotFoundError()
    {
        var petId = Guid.NewGuid();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns((Pet?)null);

        var result = await _removePetImageCommandHandler.Handle(
            new RemovePetImageCommand(petId, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<NotFoundError>();
    }

    [Fact]
    public async Task Handle_When_ImageNotFoundOnPet_Returns_NotFoundError()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)pet.OwnerId);
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        var command = new RemovePetImageCommand(petId, Guid.NewGuid());

        var result = await _removePetImageCommandHandler.Handle(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<NotFoundError>();
    }

    [Fact]
    public async Task Handle_When_PetBelongsToAnotherOwner_Returns_ForbiddenError()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)Guid.NewGuid());
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        var result = await _removePetImageCommandHandler.Handle(
            new RemovePetImageCommand(petId, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ForbiddenError>();
    }

    [Fact]
    public async Task Handle_When_AdminRemovesImageFromAnotherOwnersPet_Deletes_BlobAndRemovesFromPet()
    {
        var petId = Guid.NewGuid();
        var petImage = new PetImageBuilder().Build();
        var pet = new PetBuilder().WithImage(petImage).Build();
        _currentUserService.OwnerId.Returns((Guid?)Guid.NewGuid()); // different user
        _currentUserService.IsAdmin.Returns(true);
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        var command = new RemovePetImageCommand(petId, petImage.Id);

        await _removePetImageCommandHandler.Handle(command, CancellationToken.None);

        await _blobStorageService.Received(1).DeleteAsync(
            "pet-images", petImage.BlobName, CancellationToken.None);
        pet.Images.ShouldBeEmpty();
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }
}
