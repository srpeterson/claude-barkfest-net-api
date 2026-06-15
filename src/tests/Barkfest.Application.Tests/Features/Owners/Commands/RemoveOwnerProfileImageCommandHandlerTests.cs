using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Owners.Commands.RemoveOwnerProfileImage;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Owners.Commands;

public class RemoveOwnerProfileImageCommandHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly IBlobStorageService _blobStorageService = Substitute.For<IBlobStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly RemoveOwnerProfileImageCommandHandler _removeOwnerProfileImageCommandHandler;

    public RemoveOwnerProfileImageCommandHandlerTests()
    {
        _removeOwnerProfileImageCommandHandler = new RemoveOwnerProfileImageCommandHandler(_ownerRepository, _blobStorageService, _unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_When_OwnerHasImage_Deletes_BlobAndClearsImage()
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        owner.SetProfileImage("owners/abc/photo.jpg", "image/jpeg");
        _currentUserService.OwnerId.Returns((Guid?)owner.Id);
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);

        await _removeOwnerProfileImageCommandHandler.Handle(new RemoveOwnerProfileImageCommand(ownerId), CancellationToken.None);

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
        _currentUserService.OwnerId.Returns((Guid?)owner.Id);
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);

        await _removeOwnerProfileImageCommandHandler.Handle(new RemoveOwnerProfileImageCommand(ownerId), CancellationToken.None);

        await _blobStorageService.DidNotReceive().DeleteAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_OwnerNotFound_Returns_NotFoundError()
    {
        var ownerId = Guid.NewGuid();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns((Owner?)null);

        var result = await _removeOwnerProfileImageCommandHandler.Handle(
            new RemoveOwnerProfileImageCommand(ownerId), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<NotFoundError>();
    }

    [Fact]
    public async Task Handle_When_OwnerIsNotCurrentUser_Returns_ForbiddenError()
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)Guid.NewGuid());
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);

        var result = await _removeOwnerProfileImageCommandHandler.Handle(
            new RemoveOwnerProfileImageCommand(ownerId), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ForbiddenError>();
    }
}
