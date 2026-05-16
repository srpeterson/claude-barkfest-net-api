using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Owners.Commands.UploadOwnerProfileImage;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Owners.Commands;

public class UploadOwnerProfileImageCommandHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly IBlobStorageService _blobStorageService = Substitute.For<IBlobStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UploadOwnerProfileImageCommandHandler _sut;

    public UploadOwnerProfileImageCommandHandlerTests()
    {
        _sut = new UploadOwnerProfileImageCommandHandler(_ownerRepository, _blobStorageService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_OwnerWithoutExistingImage_UploadsAndSaves()
    {
        var ownerId = Guid.NewGuid();
        var owner = BuildOwner();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);
        var content = new MemoryStream([0x89, 0x50]);

        var command = new UploadOwnerProfileImageCommand(ownerId, "photo.jpg", content, "image/jpeg");

        await _sut.Handle(command, CancellationToken.None);

        await _blobStorageService.DidNotReceive().DeleteAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _blobStorageService.Received(1).UploadAsync(
            "owner-profile-images",
            Arg.Is<string>(b => b.StartsWith($"owners/{ownerId}/")),
            content,
            "image/jpeg",
            CancellationToken.None);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_OwnerWithExistingImage_DeletesOldThenUploadsNew()
    {
        var ownerId = Guid.NewGuid();
        var owner = BuildOwner();
        owner.SetProfileImage("owners/old/blob.jpg", "image/jpeg");
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);
        var content = new MemoryStream([0x89, 0x50]);

        var command = new UploadOwnerProfileImageCommand(ownerId, "new.jpg", content, "image/jpeg");

        await _sut.Handle(command, CancellationToken.None);

        await _blobStorageService.Received(1).DeleteAsync(
            "owner-profile-images", "owners/old/blob.jpg", CancellationToken.None);
        await _blobStorageService.Received(1).UploadAsync(
            "owner-profile-images",
            Arg.Is<string>(b => b.StartsWith($"owners/{ownerId}/")),
            content,
            "image/jpeg",
            CancellationToken.None);
    }

    [Fact]
    public async Task Handle_OwnerNotFound_ThrowsNotFoundException()
    {
        var ownerId = Guid.NewGuid();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns((Owner?)null);

        var command = new UploadOwnerProfileImageCommand(ownerId, "photo.jpg", Stream.Null, "image/jpeg");

        await Should.ThrowAsync<NotFoundException>(() => _sut.Handle(command, CancellationToken.None));
    }

    private static Owner BuildOwner()
    {
        var owner = new Owner();
        owner.SetFirstName("John");
        owner.SetLastName("Doe");
        owner.SetEmail("john@example.com");
        return owner;
    }
}
