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
    public async Task Handle_When_PetHasImage_Deletes_BlobAndClearsImage()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        pet.SetProfileImage("pets/abc/photo.jpg", "image/jpeg");
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        await _sut.Handle(new RemovePetProfileImageCommand(petId), CancellationToken.None);

        await _blobStorageService.Received(1).DeleteAsync(
            "pet-profile-images", "pets/abc/photo.jpg", CancellationToken.None);
        pet.ProfileImage.ShouldBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_PetHasNoImage_Skips_BlobDeleteAndSaves()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        await _sut.Handle(new RemovePetProfileImageCommand(petId), CancellationToken.None);

        await _blobStorageService.DidNotReceive().DeleteAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_PetNotFound_Throws_NotFoundException()
    {
        var petId = Guid.NewGuid();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns((Pet?)null);

        await Should.ThrowAsync<NotFoundException>(
            () => _sut.Handle(new RemovePetProfileImageCommand(petId), CancellationToken.None));
    }

}
