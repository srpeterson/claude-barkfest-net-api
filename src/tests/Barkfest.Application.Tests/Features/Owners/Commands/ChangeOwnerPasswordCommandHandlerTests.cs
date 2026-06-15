using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Owners.Commands.ChangeOwnerPassword;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Owners.Commands;

public class ChangeOwnerPasswordCommandHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly ChangeOwnerPasswordCommandHandler _changeOwnerPasswordCommandHandler;

    public ChangeOwnerPasswordCommandHandlerTests()
    {
        _changeOwnerPasswordCommandHandler = new ChangeOwnerPasswordCommandHandler(
            _ownerRepository, _passwordHasher, _unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_When_CurrentPasswordIsCorrect_UpdatesHash_AndSaves()
    {
        var owner = new OwnerBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)owner.Id);
        _ownerRepository.GetByIdAsync(owner.Id, CancellationToken.None).Returns(owner);
        _passwordHasher.Verify("OldPassword1!", owner.PasswordHash).Returns(true);
        _passwordHasher.Hash("NewPassword1!").Returns("new-hash");

        await _changeOwnerPasswordCommandHandler.Handle(
            new ChangeOwnerPasswordCommand(owner.Id, "OldPassword1!", "NewPassword1!"),
            CancellationToken.None);

        _passwordHasher.Received(1).Hash("NewPassword1!");
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_CallerIsNotOwner_Returns_ForbiddenError()
    {
        var ownerId = Guid.NewGuid();
        _currentUserService.OwnerId.Returns((Guid?)Guid.NewGuid());

        var result = await _changeOwnerPasswordCommandHandler.Handle(
            new ChangeOwnerPasswordCommand(ownerId, "OldPassword1!", "NewPassword1!"),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ForbiddenError>();
    }

    [Fact]
    public async Task Handle_When_OwnerNotFound_Returns_NotFoundError()
    {
        var ownerId = Guid.NewGuid();
        _currentUserService.OwnerId.Returns((Guid?)ownerId);
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns((Owner?)null);

        var result = await _changeOwnerPasswordCommandHandler.Handle(
            new ChangeOwnerPasswordCommand(ownerId, "OldPassword1!", "NewPassword1!"),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<NotFoundError>();
    }

    [Fact]
    public async Task Handle_When_CurrentPasswordIsWrong_Returns_ForbiddenError()
    {
        var owner = new OwnerBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)owner.Id);
        _ownerRepository.GetByIdAsync(owner.Id, CancellationToken.None).Returns(owner);
        _passwordHasher.Verify("WrongPassword1!", owner.PasswordHash).Returns(false);

        var result = await _changeOwnerPasswordCommandHandler.Handle(
            new ChangeOwnerPasswordCommand(owner.Id, "WrongPassword1!", "NewPassword1!"),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ForbiddenError>();
    }
}
