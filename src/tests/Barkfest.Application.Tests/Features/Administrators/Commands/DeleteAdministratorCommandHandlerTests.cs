using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Administrators.Commands.DeleteAdministrator;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Administrators.Commands;

public class DeleteAdministratorCommandHandlerTests
{
    private readonly IAdministratorRepository _administratorRepository = Substitute.For<IAdministratorRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly DeleteAdministratorCommandHandler _deleteAdministratorCommandHandler;

    public DeleteAdministratorCommandHandlerTests()
    {
        _deleteAdministratorCommandHandler = new DeleteAdministratorCommandHandler(
            _administratorRepository, _unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_When_AdminDeletesOther_Deletes_AndSaves()
    {
        var callerId = Guid.NewGuid();
        var target = new Administrator();
        target.SetEmail("target@barkfest.dev");
        target.SetPasswordHash("$2a$11$hash");
        _currentUserService.IsAdmin.Returns(true);
        _currentUserService.AdminId.Returns((Guid?)callerId);
        _administratorRepository.GetByIdAsync(target.Id, CancellationToken.None).Returns(target);

        await _deleteAdministratorCommandHandler.Handle(
            new DeleteAdministratorCommand(target.Id), CancellationToken.None);

        await _administratorRepository.Received(1).DeleteAsync(target, CancellationToken.None);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_AdminDeletesSelf_Throws_ForbiddenException()
    {
        var adminId = Guid.NewGuid();
        _currentUserService.IsAdmin.Returns(true);
        _currentUserService.AdminId.Returns((Guid?)adminId);

        await Should.ThrowAsync<ForbiddenException>(
            () => _deleteAdministratorCommandHandler.Handle(
                new DeleteAdministratorCommand(adminId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_CallerIsNotAdmin_Throws_ForbiddenException()
    {
        await Should.ThrowAsync<ForbiddenException>(
            () => _deleteAdministratorCommandHandler.Handle(
                new DeleteAdministratorCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_AdministratorNotFound_Throws_NotFoundException()
    {
        var callerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        _currentUserService.IsAdmin.Returns(true);
        _currentUserService.AdminId.Returns((Guid?)callerId);
        _administratorRepository.GetByIdAsync(targetId, CancellationToken.None).Returns((Administrator?)null);

        await Should.ThrowAsync<NotFoundException>(
            () => _deleteAdministratorCommandHandler.Handle(
                new DeleteAdministratorCommand(targetId), CancellationToken.None));
    }
}
