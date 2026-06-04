using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Administrators.Commands.UpdateAdministratorPassword;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Administrators.Commands;

public class UpdateAdministratorPasswordCommandHandlerTests
{
    private readonly IAdministratorRepository _administratorRepository = Substitute.For<IAdministratorRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly UpdateAdministratorPasswordCommandHandler _updateAdministratorPasswordCommandHandler;

    public UpdateAdministratorPasswordCommandHandlerTests()
    {
        _updateAdministratorPasswordCommandHandler = new UpdateAdministratorPasswordCommandHandler(
            _administratorRepository, _unitOfWork, _passwordHasher, _currentUserService);
    }

    [Fact]
    public async Task Handle_When_AdminUpdatesPassword_Updates_AndSaves()
    {
        var administrator = new Administrator();
        administrator.SetEmail("target@barkfest.dev");
        administrator.SetPasswordHash("$2a$11$oldhash");
        _currentUserService.IsAdmin.Returns(true);
        _administratorRepository.GetByIdAsync(administrator.Id, CancellationToken.None).Returns(administrator);
        _passwordHasher.Hash("newpassword").Returns("$2a$11$newhash");

        await _updateAdministratorPasswordCommandHandler.Handle(
            new UpdateAdministratorPasswordCommand(administrator.Id, "newpassword"), CancellationToken.None);

        administrator.PasswordHash.ShouldBe("$2a$11$newhash");
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_CallerIsNotAdmin_Throws_ForbiddenException()
    {
        await Should.ThrowAsync<ForbiddenException>(
            () => _updateAdministratorPasswordCommandHandler.Handle(
                new UpdateAdministratorPasswordCommand(Guid.NewGuid(), "newpassword"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_AdministratorNotFound_Throws_NotFoundException()
    {
        var id = Guid.NewGuid();
        _currentUserService.IsAdmin.Returns(true);
        _administratorRepository.GetByIdAsync(id, CancellationToken.None).Returns((Administrator?)null);

        await Should.ThrowAsync<NotFoundException>(
            () => _updateAdministratorPasswordCommandHandler.Handle(
                new UpdateAdministratorPasswordCommand(id, "newpassword"), CancellationToken.None));
    }
}
