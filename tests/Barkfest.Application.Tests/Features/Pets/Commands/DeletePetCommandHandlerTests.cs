using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Pets.Commands.DeletePet;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class DeletePetCommandHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IBlobStorageService _blobStorageService = Substitute.For<IBlobStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly DeletePetCommandHandler _deletePetCommandHandler;

    public DeletePetCommandHandlerTests()
    {
        _deletePetCommandHandler = new DeletePetCommandHandler(
            _petRepository, _blobStorageService, _unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_When_PetExists_Deletes_AndSaves()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)pet.OwnerId);
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        await _deletePetCommandHandler.Handle(new DeletePetCommand(petId), CancellationToken.None);

        await _petRepository.Received(1).DeleteAsync(petId, CancellationToken.None);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_PetHasImages_DeletesAllBlobs()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        var image1 = new PetImageBuilder().WithBlobName("pets/abc/gallery/photo1.jpg").WithDisplayOrder(0).Build();
        var image2 = new PetImageBuilder().WithBlobName("pets/abc/gallery/photo2.jpg").WithDisplayOrder(1).Build();
        pet.AddImage(image1);
        pet.AddImage(image2);
        _currentUserService.OwnerId.Returns((Guid?)pet.OwnerId);
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        await _deletePetCommandHandler.Handle(new DeletePetCommand(petId), CancellationToken.None);

        await _blobStorageService.Received(1).DeleteAsync("pet-images", "pets/abc/gallery/photo1.jpg", CancellationToken.None);
        await _blobStorageService.Received(1).DeleteAsync("pet-images", "pets/abc/gallery/photo2.jpg", CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_PetNotFound_Throws_NotFoundException()
    {
        var petId = Guid.NewGuid();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns((Pet?)null);

        await Should.ThrowAsync<NotFoundException>(
            () => _deletePetCommandHandler.Handle(new DeletePetCommand(petId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_PetBelongsToAnotherOwner_Throws_ForbiddenException()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)Guid.NewGuid());
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        await Should.ThrowAsync<ForbiddenException>(
            () => _deletePetCommandHandler.Handle(new DeletePetCommand(petId), CancellationToken.None));
    }
}
