using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Pets.Queries.GetPetsByOwnerId;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Queries;

public class GetPetsByOwnerIdQueryHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly GetPetsByOwnerIdQueryHandler _getPetsByOwnerIdQueryHandler;

    public GetPetsByOwnerIdQueryHandlerTests()
    {
        _getPetsByOwnerIdQueryHandler = new GetPetsByOwnerIdQueryHandler(
            _petRepository, _ownerRepository, _currentUserService);
    }

    [Fact]
    public async Task Handle_When_OwnerHasPets_Returns_PetsMappedToDtos()
    {
        var owner = new OwnerBuilder().Build();
        var pets = new[]
        {
            new PetBuilder().WithOwnerId(owner.Id).WithName("Max").Build(),
            new PetBuilder().WithOwnerId(owner.Id).WithName("Daisy").Build()
        };
        _ownerRepository.GetByIdAsync(owner.Id, CancellationToken.None).Returns(owner);
        _petRepository.GetByOwnerIdAsync(owner.Id, CancellationToken.None).Returns(pets);

        var result = await _getPetsByOwnerIdQueryHandler.Handle(
            new GetPetsByOwnerIdQuery(owner.Id), CancellationToken.None);

        var list = result.ToList();
        list.Count.ShouldBe(2);
        list.ShouldAllBe(p => p.OwnerId == owner.Id);
    }

    [Fact]
    public async Task Handle_When_OwnerHasNoPets_Returns_EmptyCollection()
    {
        var owner = new OwnerBuilder().Build();
        _ownerRepository.GetByIdAsync(owner.Id, CancellationToken.None).Returns(owner);
        _petRepository.GetByOwnerIdAsync(owner.Id, CancellationToken.None).Returns([]);

        var result = await _getPetsByOwnerIdQueryHandler.Handle(
            new GetPetsByOwnerIdQuery(owner.Id), CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_When_OwnerIsInactive_Throws_NotFoundException()
    {
        var owner = new OwnerBuilder().Build();
        owner.SetIsActive(false);
        _ownerRepository.GetByIdAsync(owner.Id, CancellationToken.None).Returns(owner);
        // IsAdmin returns false by default (NSubstitute default for bool)

        await Should.ThrowAsync<NotFoundException>(
            () => _getPetsByOwnerIdQueryHandler.Handle(
                new GetPetsByOwnerIdQuery(owner.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_OwnerIsInactive_And_CallerIsAdmin_Returns_Pets()
    {
        var owner = new OwnerBuilder().Build();
        owner.SetIsActive(false);
        var pets = new[] { new PetBuilder().WithOwnerId(owner.Id).Build() };
        _ownerRepository.GetByIdAsync(owner.Id, CancellationToken.None).Returns(owner);
        _petRepository.GetByOwnerIdAsync(owner.Id, CancellationToken.None).Returns(pets);
        _currentUserService.IsAdmin.Returns(true);

        var result = await _getPetsByOwnerIdQueryHandler.Handle(
            new GetPetsByOwnerIdQuery(owner.Id), CancellationToken.None);

        result.Count().ShouldBe(1);
    }

    [Fact]
    public async Task Handle_When_OwnerNotFound_Throws_NotFoundException()
    {
        var ownerId = Guid.NewGuid();
        _ownerRepository.GetByIdAsync(ownerId, CancellationToken.None).Returns((Owner?)null);

        await Should.ThrowAsync<NotFoundException>(
            () => _getPetsByOwnerIdQueryHandler.Handle(
                new GetPetsByOwnerIdQuery(ownerId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_OwnerIsInvisible_Throws_NotFoundException()
    {
        var owner = new OwnerBuilder().Build();
        owner.SetIsVisible(false);
        _ownerRepository.GetByIdAsync(owner.Id, CancellationToken.None).Returns(owner);
        // IsAdmin returns false and OwnerId returns Guid.Empty by default (NSubstitute)

        await Should.ThrowAsync<NotFoundException>(
            () => _getPetsByOwnerIdQueryHandler.Handle(
                new GetPetsByOwnerIdQuery(owner.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_OwnerIsInvisible_And_CallerIsOwner_Returns_Pets()
    {
        var owner = new OwnerBuilder().Build();
        owner.SetIsVisible(false);
        var pets = new[] { new PetBuilder().WithOwnerId(owner.Id).Build() };
        _ownerRepository.GetByIdAsync(owner.Id, CancellationToken.None).Returns(owner);
        _petRepository.GetByOwnerIdAsync(owner.Id, CancellationToken.None).Returns(pets);
        _currentUserService.OwnerId.Returns((Guid?)owner.Id);

        var result = await _getPetsByOwnerIdQueryHandler.Handle(
            new GetPetsByOwnerIdQuery(owner.Id), CancellationToken.None);

        result.Count().ShouldBe(1);
    }

    [Fact]
    public async Task Handle_When_OwnerIsInvisible_And_CallerIsAdmin_Returns_Pets()
    {
        var owner = new OwnerBuilder().Build();
        owner.SetIsVisible(false);
        var pets = new[] { new PetBuilder().WithOwnerId(owner.Id).Build() };
        _ownerRepository.GetByIdAsync(owner.Id, CancellationToken.None).Returns(owner);
        _petRepository.GetByOwnerIdAsync(owner.Id, CancellationToken.None).Returns(pets);
        _currentUserService.IsAdmin.Returns(true);

        var result = await _getPetsByOwnerIdQueryHandler.Handle(
            new GetPetsByOwnerIdQuery(owner.Id), CancellationToken.None);

        result.Count().ShouldBe(1);
    }
}
