using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Owners.Commands.SetOwnerVisibility;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Owners.Commands;

public class SetOwnerVisibilityCommandHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly SetOwnerVisibilityCommandHandler _setOwnerVisibilityCommandHandler;

    public SetOwnerVisibilityCommandHandlerTests()
    {
        _setOwnerVisibilityCommandHandler = new SetOwnerVisibilityCommandHandler(
            _ownerRepository, _unitOfWork, _currentUserService);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_When_OwnerSetsVisibility_Updates_AndSaves(bool isVisible)
    {
        var owner = new OwnerBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)owner.Id);
        _ownerRepository.GetByIdAsync(owner.Id, CancellationToken.None).Returns(owner);

        await _setOwnerVisibilityCommandHandler.Handle(
            new SetOwnerVisibilityCommand(owner.Id, isVisible), CancellationToken.None);

        owner.IsVisible.ShouldBe(isVisible);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_CallerIsNotOwner_Throws_ForbiddenException()
    {
        var owner = new OwnerBuilder().Build();
        _ownerRepository.GetByIdAsync(owner.Id, CancellationToken.None).Returns(owner);
        // OwnerId returns Guid.Empty by default (NSubstitute default for Guid)

        await Should.ThrowAsync<ForbiddenException>(
            () => _setOwnerVisibilityCommandHandler.Handle(
                new SetOwnerVisibilityCommand(owner.Id, false), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_OwnerNotFound_Throws_NotFoundException()
    {
        var ownerId = Guid.NewGuid();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns((Owner?)null);

        await Should.ThrowAsync<NotFoundException>(
            () => _setOwnerVisibilityCommandHandler.Handle(
                new SetOwnerVisibilityCommand(ownerId, false), CancellationToken.None));
    }
}
