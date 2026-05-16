using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Features.Owners.Queries.GetOwnerById;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Owners.Queries;

public class GetOwnerByIdQueryHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly GetOwnerByIdQueryHandler _sut;

    public GetOwnerByIdQueryHandlerTests()
    {
        _sut = new GetOwnerByIdQueryHandler(_ownerRepository);
    }

    [Fact]
    public async Task Handle_ExistingOwner_ReturnsMappedDto()
    {
        var ownerId = Guid.NewGuid();
        var owner = BuildOwner();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);

        var result = await _sut.Handle(new GetOwnerByIdQuery(ownerId), CancellationToken.None);

        result.FirstName.ShouldBe("John");
        result.LastName.ShouldBe("Doe");
        result.Email.ShouldBe("john@example.com");
    }

    [Fact]
    public async Task Handle_OwnerNotFound_ThrowsNotFoundException()
    {
        var ownerId = Guid.NewGuid();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns((Owner?)null);

        await Should.ThrowAsync<NotFoundException>(
            () => _sut.Handle(new GetOwnerByIdQuery(ownerId), CancellationToken.None));
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
