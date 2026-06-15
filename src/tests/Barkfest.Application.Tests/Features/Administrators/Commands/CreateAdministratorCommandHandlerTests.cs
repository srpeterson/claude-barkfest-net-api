using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Administrators.Commands.CreateAdministrator;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Administrators.Commands;

public class CreateAdministratorCommandHandlerTests
{
    private readonly IAdministratorRepository _administratorRepository = Substitute.For<IAdministratorRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly CreateAdministratorCommandHandler _createAdministratorCommandHandler;

    public CreateAdministratorCommandHandlerTests()
    {
        _createAdministratorCommandHandler = new CreateAdministratorCommandHandler(
            _administratorRepository, _unitOfWork, _passwordHasher, _currentUserService);
    }

    [Fact]
    public async Task Handle_When_AdminCreatesAdministrator_Returns_NewAdministratorId()
    {
        _currentUserService.IsAdmin.Returns(true);
        _administratorRepository.GetByUsernameAsync("newadmin", CancellationToken.None).Returns((Administrator?)null);
        _administratorRepository.GetByEmailAsync("new@barkfest.dev", CancellationToken.None).Returns((Administrator?)null);
        _passwordHasher.Hash("securepass").Returns("$2a$11$hash");

        var result = await _createAdministratorCommandHandler.Handle(
            new CreateAdministratorCommand("newadmin", "New Admin", "new@barkfest.dev", "+15555550100", "securepass"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
        await _administratorRepository.Received(1).AddAsync(
            Arg.Is<Administrator>(a => a.Username == "newadmin" && a.Name == "New Admin" && a.Email == "new@barkfest.dev"),
            CancellationToken.None);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_CallerIsNotAdmin_Throws_ForbiddenException()
    {
        // IsAdmin returns false by default (NSubstitute default for bool)

        var result = await _createAdministratorCommandHandler.Handle(
            new CreateAdministratorCommand("newadmin", "New Admin", "new@barkfest.dev", "+15555550100", "securepass"), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ForbiddenError>();
    }

    [Fact]
    public async Task Handle_When_UsernameAlreadyInUse_Throws_DomainException()
    {
        _currentUserService.IsAdmin.Returns(true);
        var existing = new Administrator();
        existing.SetUsername("takenuser");
        existing.SetEmail("existing@barkfest.dev");
        existing.SetPasswordHash("$2a$11$hash");
        _administratorRepository.GetByUsernameAsync("takenuser", CancellationToken.None).Returns(existing);

        var result = await _createAdministratorCommandHandler.Handle(
            new CreateAdministratorCommand("takenuser", "New Admin", "new@barkfest.dev", "+15555550100", "securepass"), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<DomainRuleError>();
    }

    [Fact]
    public async Task Handle_When_EmailAlreadyInUse_Throws_DomainException()
    {
        _currentUserService.IsAdmin.Returns(true);
        _administratorRepository.GetByUsernameAsync("newadmin", CancellationToken.None).Returns((Administrator?)null);
        var existing = new Administrator();
        existing.SetUsername("otheradmin");
        existing.SetEmail("existing@barkfest.dev");
        existing.SetPasswordHash("$2a$11$hash");
        _administratorRepository.GetByEmailAsync("existing@barkfest.dev", CancellationToken.None).Returns(existing);

        var result = await _createAdministratorCommandHandler.Handle(
            new CreateAdministratorCommand("newadmin", "New Admin", "existing@barkfest.dev", "+15555550100", "securepass"), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<DomainRuleError>();
    }
}
