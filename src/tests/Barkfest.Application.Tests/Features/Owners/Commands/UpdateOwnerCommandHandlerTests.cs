using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Owners.Commands.UpdateOwner;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Owners.Commands;

public class UpdateOwnerCommandHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly UpdateOwnerCommandHandler _updateOwnerCommandHandler;

    public UpdateOwnerCommandHandlerTests()
    {
        _updateOwnerCommandHandler = new UpdateOwnerCommandHandler(_ownerRepository, _unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_When_OwnerExists_Updates_AndSaves()
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().WithFirstName("Original").WithLastName("Owner").WithEmail("original@example.com").Build();
        _currentUserService.OwnerId.Returns((Guid?)owner.Id);
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);

        var command = new UpdateOwnerCommand(ownerId, "Updated", "Name", "updated@example.com", null);

        await _updateOwnerCommandHandler.Handle(command, CancellationToken.None);

        await _ownerRepository.Received(1).UpdateAsync(
            Arg.Is<Owner>(o => o.FirstName == "Updated" && o.Email == "updated@example.com"),
            CancellationToken.None);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_OwnerNotFound_Returns_NotFoundError()
    {
        var ownerId = Guid.NewGuid();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns((Owner?)null);

        var command = new UpdateOwnerCommand(ownerId, "John", "Doe", "john@example.com", null);

        var result = await _updateOwnerCommandHandler.Handle(command, CancellationToken.None);

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

        var command = new UpdateOwnerCommand(ownerId, "John", "Doe", "john@example.com", null);

        var result = await _updateOwnerCommandHandler.Handle(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ForbiddenError>();
    }
}
