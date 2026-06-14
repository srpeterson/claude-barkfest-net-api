using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Owners.Commands.DeleteOwner;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Owners.Commands;

public class DeleteOwnerCommandHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly DeleteOwnerCommandHandler _deleteOwnerCommandHandler;

    public DeleteOwnerCommandHandlerTests()
    {
        _deleteOwnerCommandHandler = new DeleteOwnerCommandHandler(_ownerRepository, _unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_When_OwnerExists_Deletes_AndSaves()
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)owner.Id);
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);

        await _deleteOwnerCommandHandler.Handle(new DeleteOwnerCommand(ownerId), CancellationToken.None);

        await _ownerRepository.Received(1).DeleteAsync(ownerId, CancellationToken.None);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_OwnerNotFound_Throws_NotFoundException()
    {
        var ownerId = Guid.NewGuid();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns((Owner?)null);

        var result = await _deleteOwnerCommandHandler.Handle(new DeleteOwnerCommand(ownerId), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<NotFoundError>();
    }

    [Fact]
    public async Task Handle_When_OwnerIsNotCurrentUser_Throws_ForbiddenException()
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)Guid.NewGuid());
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);

        var result = await _deleteOwnerCommandHandler.Handle(new DeleteOwnerCommand(ownerId), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ForbiddenError>();
    }

    [Fact]
    public async Task Handle_When_AdminDeletesAnotherOwner_Deletes_AndSaves()
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)Guid.NewGuid()); // different user
        _currentUserService.IsAdmin.Returns(true);
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);

        await _deleteOwnerCommandHandler.Handle(new DeleteOwnerCommand(ownerId), CancellationToken.None);

        await _ownerRepository.Received(1).DeleteAsync(ownerId, CancellationToken.None);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }
}
