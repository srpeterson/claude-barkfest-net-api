using Barkfest.Application.Features.Owners.Commands.CreateOwner;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Owners.Commands;

public class CreateOwnerCommandHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateOwnerCommandHandler _createOwnerCommandHandler;

    public CreateOwnerCommandHandlerTests()
    {
        _createOwnerCommandHandler = new CreateOwnerCommandHandler(_ownerRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_When_CommandIsValid_Returns_ValidGuid()
    {
        var command = new CreateOwnerCommand("John", "Doe", "john@example.com", null);

        var result = await _createOwnerCommandHandler.Handle(command, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_When_CommandIsValid_Adds_OwnerToRepository()
    {
        var command = new CreateOwnerCommand("Jane", "Smith", "jane@example.com", "555-0100");

        await _createOwnerCommandHandler.Handle(command, CancellationToken.None);

        await _ownerRepository.Received(1).AddAsync(
            Arg.Is<Owner>(o =>
                o.FirstName == "Jane" &&
                o.LastName == "Smith" &&
                o.Email == "jane@example.com"),
            CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_CommandIsValid_Saves_Changes()
    {
        var command = new CreateOwnerCommand("John", "Doe", "john@example.com", null);

        await _createOwnerCommandHandler.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }
}
