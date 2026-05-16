using Barkfest.Application.Features.Owners.Commands.CreateOwner;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Owners.Commands;

public class CreateOwnerCommandHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateOwnerCommandHandler _sut;

    public CreateOwnerCommandHandlerTests()
    {
        _sut = new CreateOwnerCommandHandler(_ownerRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNonEmptyGuid()
    {
        var command = new CreateOwnerCommand("John", "Doe", "john@example.com", null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsOwnerToRepository()
    {
        var command = new CreateOwnerCommand("Jane", "Smith", "jane@example.com", "555-0100");

        await _sut.Handle(command, CancellationToken.None);

        await _ownerRepository.Received(1).AddAsync(
            Arg.Is<Owner>(o =>
                o.FirstName == "Jane" &&
                o.LastName == "Smith" &&
                o.Email == "jane@example.com"),
            CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        var command = new CreateOwnerCommand("John", "Doe", "john@example.com", null);

        await _sut.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }
}
