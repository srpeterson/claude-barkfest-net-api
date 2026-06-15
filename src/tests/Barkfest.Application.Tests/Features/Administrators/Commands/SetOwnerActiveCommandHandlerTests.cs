using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Administrators.Commands.SetOwnerActive;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Administrators.Commands;

public class SetOwnerActiveCommandHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly SetOwnerActiveCommandHandler _setOwnerActiveCommandHandler;

    public SetOwnerActiveCommandHandlerTests()
    {
        _setOwnerActiveCommandHandler = new SetOwnerActiveCommandHandler(
            _ownerRepository, _unitOfWork, _currentUserService);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_When_AdminSetsOwnerActive_Updates_AndSaves(bool active)
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        _currentUserService.IsAdmin.Returns(true);
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);

        await _setOwnerActiveCommandHandler.Handle(
            new SetOwnerActiveCommand(ownerId, active), CancellationToken.None);

        owner.IsActive.ShouldBe(active);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_CallerIsNotAdmin_Returns_ForbiddenError()
    {
        var ownerId = Guid.NewGuid();
        // IsAdmin returns false by default (NSubstitute default for bool)

        var result = await _setOwnerActiveCommandHandler.Handle(
            new SetOwnerActiveCommand(ownerId, false), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ForbiddenError>();
    }

    [Fact]
    public async Task Handle_When_OwnerNotFound_Returns_NotFoundError()
    {
        var ownerId = Guid.NewGuid();
        _currentUserService.IsAdmin.Returns(true);
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns((Owner?)null);

        var result = await _setOwnerActiveCommandHandler.Handle(
            new SetOwnerActiveCommand(ownerId, false), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<NotFoundError>();
    }
}
