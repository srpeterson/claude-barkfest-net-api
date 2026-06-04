using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Owners.Commands.UploadOwnerProfileImage;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Owners.Commands;

public class UploadOwnerProfileImageCommandHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly IBlobStorageService _blobStorageService = Substitute.For<IBlobStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IContentModerationService _contentModerationService = Substitute.For<IContentModerationService>();
    private readonly UploadOwnerProfileImageCommandHandler _uploadOwnerProfileImageCommandHandler;

    public UploadOwnerProfileImageCommandHandlerTests()
    {
        _contentModerationService.IsImageSafeAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(true);
        _uploadOwnerProfileImageCommandHandler = new UploadOwnerProfileImageCommandHandler(_ownerRepository, _blobStorageService, _unitOfWork, _currentUserService, _contentModerationService);
    }

    [Fact]
    public async Task Handle_When_ImageFailsModeration_Throws_DomainException()
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)owner.Id);
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);
        _contentModerationService.IsImageSafeAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(false);

        var command = new UploadOwnerProfileImageCommand(ownerId, "photo.jpg", Stream.Null, "image/jpeg");

        await Should.ThrowAsync<DomainException>(() => _uploadOwnerProfileImageCommandHandler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_OwnerHasNoExistingImage_Uploads_AndSaves()
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)owner.Id);
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);
        var content = new MemoryStream([0x89, 0x50]);

        var command = new UploadOwnerProfileImageCommand(ownerId, "photo.jpg", content, "image/jpeg");

        await _uploadOwnerProfileImageCommandHandler.Handle(command, CancellationToken.None);

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
    public async Task Handle_When_OwnerHasExistingImage_Deletes_OldThenUploadsNew()
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        owner.SetProfileImage("owners/old/blob.jpg", "image/jpeg");
        _currentUserService.OwnerId.Returns((Guid?)owner.Id);
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);
        var content = new MemoryStream([0x89, 0x50]);

        var command = new UploadOwnerProfileImageCommand(ownerId, "new.jpg", content, "image/jpeg");

        await _uploadOwnerProfileImageCommandHandler.Handle(command, CancellationToken.None);

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
    public async Task Handle_When_OwnerNotFound_Throws_NotFoundException()
    {
        var ownerId = Guid.NewGuid();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns((Owner?)null);

        var command = new UploadOwnerProfileImageCommand(ownerId, "photo.jpg", Stream.Null, "image/jpeg");

        await Should.ThrowAsync<NotFoundException>(() => _uploadOwnerProfileImageCommandHandler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_OwnerIsNotCurrentUser_Throws_ForbiddenException()
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)Guid.NewGuid());
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);

        var command = new UploadOwnerProfileImageCommand(ownerId, "photo.jpg", Stream.Null, "image/jpeg");

        await Should.ThrowAsync<ForbiddenException>(() => _uploadOwnerProfileImageCommandHandler.Handle(command, CancellationToken.None));
    }
}
