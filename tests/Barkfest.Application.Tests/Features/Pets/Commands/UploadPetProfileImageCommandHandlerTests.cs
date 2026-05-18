using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Pets.Commands.UploadPetProfileImage;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class UploadPetProfileImageCommandHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IBlobStorageService _blobStorageService = Substitute.For<IBlobStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UploadPetProfileImageCommandHandler _sut;

    public UploadPetProfileImageCommandHandlerTests()
    {
        _sut = new UploadPetProfileImageCommandHandler(_petRepository, _blobStorageService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_When_PetHasNoExistingImage_Uploads_AndSaves()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);
        var content = new MemoryStream([0x89, 0x50]);

        var command = new UploadPetProfileImageCommand(petId, "photo.png", content, "image/png");

        await _sut.Handle(command, CancellationToken.None);

        await _blobStorageService.DidNotReceive().DeleteAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _blobStorageService.Received(1).UploadAsync(
            "pet-profile-images",
            Arg.Is<string>(b => b.StartsWith($"pets/{petId}/profile/")),
            content,
            "image/png",
            CancellationToken.None);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_PetHasExistingImage_Deletes_OldThenUploadsNew()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        pet.SetProfileImage("pets/old/photo.jpg", "image/jpeg");
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);
        var content = new MemoryStream([0x89, 0x50]);

        var command = new UploadPetProfileImageCommand(petId, "new.jpg", content, "image/jpeg");

        await _sut.Handle(command, CancellationToken.None);

        await _blobStorageService.Received(1).DeleteAsync(
            "pet-profile-images", "pets/old/photo.jpg", CancellationToken.None);
        await _blobStorageService.Received(1).UploadAsync(
            "pet-profile-images",
            Arg.Is<string>(b => b.StartsWith($"pets/{petId}/profile/")),
            content,
            "image/jpeg",
            CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_PetNotFound_Throws_NotFoundException()
    {
        var petId = Guid.NewGuid();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns((Pet?)null);

        var command = new UploadPetProfileImageCommand(petId, "photo.jpg", Stream.Null, "image/jpeg");

        await Should.ThrowAsync<NotFoundException>(() => _sut.Handle(command, CancellationToken.None));
    }

}
