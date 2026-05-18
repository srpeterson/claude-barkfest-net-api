using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Owners.Commands.RemoveOwnerProfileImage;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Owners.Commands;

public class RemoveOwnerProfileImageCommandHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly IBlobStorageService _blobStorageService = Substitute.For<IBlobStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RemoveOwnerProfileImageCommandHandler _sut;

    public RemoveOwnerProfileImageCommandHandlerTests()
    {
        _sut = new RemoveOwnerProfileImageCommandHandler(_ownerRepository, _blobStorageService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_When_OwnerHasImage_Deletes_BlobAndClearsImage()
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        owner.SetProfileImage("owners/abc/photo.jpg", "image/jpeg");
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);

        await _sut.Handle(new RemoveOwnerProfileImageCommand(ownerId), CancellationToken.None);

        await _blobStorageService.Received(1).DeleteAsync(
            "owner-profile-images", "owners/abc/photo.jpg", CancellationToken.None);
        owner.ProfileImage.ShouldBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_OwnerHasNoImage_Skips_BlobDeleteAndSaves()
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);

        await _sut.Handle(new RemoveOwnerProfileImageCommand(ownerId), CancellationToken.None);

        await _blobStorageService.DidNotReceive().DeleteAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_OwnerNotFound_Throws_NotFoundException()
    {
        var ownerId = Guid.NewGuid();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns((Owner?)null);

        await Should.ThrowAsync<NotFoundException>(
            () => _sut.Handle(new RemoveOwnerProfileImageCommand(ownerId), CancellationToken.None));
    }

}
