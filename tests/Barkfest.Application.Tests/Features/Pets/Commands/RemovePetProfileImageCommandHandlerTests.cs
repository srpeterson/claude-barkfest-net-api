using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Pets.Commands.RemovePetProfileImage;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class RemovePetProfileImageCommandHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IBlobStorageService _blobStorageService = Substitute.For<IBlobStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RemovePetProfileImageCommandHandler _sut;

    public RemovePetProfileImageCommandHandlerTests()
    {
        _sut = new RemovePetProfileImageCommandHandler(_petRepository, _blobStorageService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_PetWithImage_DeletesBlobAndClearsImage()
    {
        var petId = Guid.NewGuid();
        var pet = BuildPet();
        pet.SetProfileImage("pets/abc/photo.jpg", "image/jpeg");
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        await _sut.Handle(new RemovePetProfileImageCommand(petId), CancellationToken.None);

        await _blobStorageService.Received(1).DeleteAsync(
            "pet-profile-images", "pets/abc/photo.jpg", CancellationToken.None);
        pet.ProfileImage.ShouldBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_PetWithoutImage_SkipsBlobDeleteAndSaves()
    {
        var petId = Guid.NewGuid();
        var pet = BuildPet();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        await _sut.Handle(new RemovePetProfileImageCommand(petId), CancellationToken.None);

        await _blobStorageService.DidNotReceive().DeleteAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_PetNotFound_ThrowsNotFoundException()
    {
        var petId = Guid.NewGuid();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns((Pet?)null);

        await Should.ThrowAsync<NotFoundException>(
            () => _sut.Handle(new RemovePetProfileImageCommand(petId), CancellationToken.None));
    }

    private static Pet BuildPet()
    {
        var pet = new Pet(Guid.NewGuid());
        pet.SetName("Buddy");
        pet.SetPetType(Barkfest.Domain.Enums.PetType.Dog);
        return pet;
    }
}
