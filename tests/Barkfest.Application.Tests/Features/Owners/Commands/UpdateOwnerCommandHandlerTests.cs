using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Features.Owners.Commands.UpdateOwner;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Owners.Commands;

public class UpdateOwnerCommandHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateOwnerCommandHandler _sut;

    public UpdateOwnerCommandHandlerTests()
    {
        _sut = new UpdateOwnerCommandHandler(_ownerRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_When_OwnerExists_Updates_AndSaves()
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().WithFirstName("Original").WithLastName("Owner").WithEmail("original@example.com").Build();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);

        var command = new UpdateOwnerCommand(ownerId, "Updated", "Name", "updated@example.com", null);

        await _sut.Handle(command, CancellationToken.None);

        await _ownerRepository.Received(1).UpdateAsync(
            Arg.Is<Owner>(o => o.FirstName == "Updated" && o.Email == "updated@example.com"),
            CancellationToken.None);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_OwnerNotFound_Throws_NotFoundException()
    {
        var ownerId = Guid.NewGuid();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns((Owner?)null);

        var command = new UpdateOwnerCommand(ownerId, "John", "Doe", "john@example.com", null);

        await Should.ThrowAsync<NotFoundException>(() => _sut.Handle(command, CancellationToken.None));
    }

}
