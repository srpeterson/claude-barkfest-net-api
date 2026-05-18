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
    public async Task Handle_When_OwnerExists_Returns_MappedDto()
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().WithFirstName("John").WithLastName("Doe").WithEmail("john@example.com").Build();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);

        var result = await _sut.Handle(new GetOwnerByIdQuery(ownerId), CancellationToken.None);

        result.FirstName.ShouldBe("John");
        result.LastName.ShouldBe("Doe");
        result.Email.ShouldBe("john@example.com");
    }

    [Fact]
    public async Task Handle_When_OwnerNotFound_Throws_NotFoundException()
    {
        var ownerId = Guid.NewGuid();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns((Owner?)null);

        await Should.ThrowAsync<NotFoundException>(
            () => _sut.Handle(new GetOwnerByIdQuery(ownerId), CancellationToken.None));
    }

}
