using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Owners.Queries.GetOwnerById;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Owners.Queries;

public class GetOwnerByIdQueryHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly GetOwnerByIdQueryHandler _getOwnerByIdQueryHandler;

    public GetOwnerByIdQueryHandlerTests()
    {
        _getOwnerByIdQueryHandler = new GetOwnerByIdQueryHandler(_ownerRepository, _currentUserService);
    }

    [Fact]
    public async Task Handle_When_OwnerExists_Returns_MappedDto()
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().WithFirstName("John").WithLastName("Doe").WithEmail("john@example.com").Build();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);

        var result = await _getOwnerByIdQueryHandler.Handle(new GetOwnerByIdQuery(ownerId), CancellationToken.None);

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
            () => _getOwnerByIdQueryHandler.Handle(new GetOwnerByIdQuery(ownerId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_OwnerIsInactive_Throws_NotFoundException()
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        owner.SetIsActive(false);
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);
        // IsAdmin returns false by default (NSubstitute default for bool)

        await Should.ThrowAsync<NotFoundException>(
            () => _getOwnerByIdQueryHandler.Handle(new GetOwnerByIdQuery(ownerId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_OwnerIsInactive_And_CallerIsAdmin_Returns_MappedDto()
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().WithFirstName("John").Build();
        owner.SetIsActive(false);
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);
        _currentUserService.IsAdmin.Returns(true);

        var result = await _getOwnerByIdQueryHandler.Handle(new GetOwnerByIdQuery(ownerId), CancellationToken.None);

        result.FirstName.ShouldBe("John");
    }

    [Fact]
    public async Task Handle_When_OwnerIsInvisible_Throws_NotFoundException()
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        owner.SetIsVisible(false);
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);
        // IsAdmin returns false and OwnerId returns Guid.Empty by default (NSubstitute)

        await Should.ThrowAsync<NotFoundException>(
            () => _getOwnerByIdQueryHandler.Handle(new GetOwnerByIdQuery(ownerId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_OwnerIsInvisible_And_CallerIsOwner_Returns_MappedDto()
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().WithFirstName("Jane").Build();
        owner.SetIsVisible(false);
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);
        _currentUserService.OwnerId.Returns((Guid?)owner.Id);

        var result = await _getOwnerByIdQueryHandler.Handle(new GetOwnerByIdQuery(ownerId), CancellationToken.None);

        result.FirstName.ShouldBe("Jane");
    }

    [Fact]
    public async Task Handle_When_OwnerIsInvisible_And_CallerIsAdmin_Returns_MappedDto()
    {
        var ownerId = Guid.NewGuid();
        var owner = new OwnerBuilder().WithFirstName("Jane").Build();
        owner.SetIsVisible(false);
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns(owner);
        _currentUserService.IsAdmin.Returns(true);

        var result = await _getOwnerByIdQueryHandler.Handle(new GetOwnerByIdQuery(ownerId), CancellationToken.None);

        result.FirstName.ShouldBe("Jane");
    }
}
