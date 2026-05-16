using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Features.Owners.Commands.DeleteOwner;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Owners.Commands;

public class DeleteOwnerCommandHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteOwnerCommandHandler _sut;

    public DeleteOwnerCommandHandlerTests()
    {
        _sut = new DeleteOwnerCommandHandler(_ownerRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ExistingOwner_DeletesAndSaves()
    {
        var ownerId = Guid.NewGuid();
        var owner = BuildOwner();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);

        await _sut.Handle(new DeleteOwnerCommand(ownerId), CancellationToken.None);

        await _ownerRepository.Received(1).DeleteAsync(ownerId, CancellationToken.None);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_OwnerNotFound_ThrowsNotFoundException()
    {
        var ownerId = Guid.NewGuid();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns((Owner?)null);

        await Should.ThrowAsync<NotFoundException>(
            () => _sut.Handle(new DeleteOwnerCommand(ownerId), CancellationToken.None));
    }

    private static Owner BuildOwner()
    {
        var owner = new Owner();
        owner.SetFirstName("John");
        owner.SetLastName("Doe");
        owner.SetEmail("john@example.com");
        return owner;
    }
}
