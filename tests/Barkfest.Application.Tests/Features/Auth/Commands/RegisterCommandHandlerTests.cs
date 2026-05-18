using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Auth.Commands.Register;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Auth.Commands;

public class RegisterCommandHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RegisterCommandHandler _registerCommandHandler;

    public RegisterCommandHandlerTests()
    {
        _registerCommandHandler = new RegisterCommandHandler(_ownerRepository, _passwordHasher, _unitOfWork);
    }

    [Fact]
    public async Task Handle_When_CommandIsValid_Returns_ValidGuid()
    {
        _ownerRepository.GetByEmailAsync("new@example.com", CancellationToken.None).Returns((Owner?)null);
        _passwordHasher.Hash("pass123").Returns("hashed-pass");

        var command = new RegisterCommand("Alice", "Adams", "new@example.com", null, "pass123");

        var result = await _registerCommandHandler.Handle(command, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_When_CommandIsValid_HashesPassword_AndSaves()
    {
        _ownerRepository.GetByEmailAsync("alice@example.com", CancellationToken.None).Returns((Owner?)null);
        _passwordHasher.Hash("mypassword").Returns("bcrypt-hash");

        var command = new RegisterCommand("Alice", "Adams", "alice@example.com", null, "mypassword");

        await _registerCommandHandler.Handle(command, CancellationToken.None);

        _passwordHasher.Received(1).Hash("mypassword");
        await _ownerRepository.Received(1).AddAsync(
            Arg.Is<Owner>(o => o.Email == "alice@example.com"),
            CancellationToken.None);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_EmailAlreadyInUse_Throws_DomainException()
    {
        var existing = new OwnerBuilder().WithEmail("taken@example.com").Build();
        _ownerRepository.GetByEmailAsync("taken@example.com", CancellationToken.None).Returns(existing);

        var command = new RegisterCommand("Bob", "Baker", "taken@example.com", null, "pass123");

        await Should.ThrowAsync<DomainException>(() => _registerCommandHandler.Handle(command, CancellationToken.None));
    }
}
