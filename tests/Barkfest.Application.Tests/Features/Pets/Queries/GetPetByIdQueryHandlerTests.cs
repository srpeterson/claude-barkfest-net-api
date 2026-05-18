using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Pets.Queries.GetPetById;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Queries;

public class GetPetByIdQueryHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly GetPetByIdQueryHandler _getPetByIdQueryHandler;

    public GetPetByIdQueryHandlerTests()
    {
        _getPetByIdQueryHandler = new GetPetByIdQueryHandler(_petRepository, _ownerRepository, _currentUserService);
    }

    [Fact]
    public async Task Handle_When_PetExists_Returns_MappedDto()
    {
        var petId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        var pet = new PetBuilder().WithOwnerId(owner.Id).Build();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);
        _ownerRepository.GetByIdAsync(owner.Id, CancellationToken.None).Returns(owner);

        var result = await _getPetByIdQueryHandler.Handle(new GetPetByIdQuery(petId), CancellationToken.None);

        result.Name.ShouldBe("Buddy");
        result.PetType.ShouldBe("Dog");
    }

    [Fact]
    public async Task Handle_When_PetNotFound_Throws_NotFoundException()
    {
        var petId = Guid.NewGuid();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns((Pet?)null);

        await Should.ThrowAsync<NotFoundException>(
            () => _getPetByIdQueryHandler.Handle(new GetPetByIdQuery(petId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_OwnerIsInactive_Throws_NotFoundException()
    {
        var petId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        owner.SetActive(false);
        var pet = new PetBuilder().WithOwnerId(owner.Id).Build();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);
        _ownerRepository.GetByIdAsync(owner.Id, CancellationToken.None).Returns(owner);
        // IsAdmin returns false by default (NSubstitute default for bool)

        await Should.ThrowAsync<NotFoundException>(
            () => _getPetByIdQueryHandler.Handle(new GetPetByIdQuery(petId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_OwnerIsInactive_And_CallerIsAdmin_Returns_MappedDto()
    {
        var petId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        owner.SetActive(false);
        var pet = new PetBuilder().WithOwnerId(owner.Id).WithName("Rex").Build();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);
        _ownerRepository.GetByIdAsync(owner.Id, CancellationToken.None).Returns(owner);
        _currentUserService.IsAdmin.Returns(true);

        var result = await _getPetByIdQueryHandler.Handle(new GetPetByIdQuery(petId), CancellationToken.None);

        result.Name.ShouldBe("Rex");
    }

    [Fact]
    public async Task Handle_When_OwnerIsInvisible_Throws_NotFoundException()
    {
        var petId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        owner.SetIsVisible(false);
        var pet = new PetBuilder().WithOwnerId(owner.Id).Build();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);
        _ownerRepository.GetByIdAsync(owner.Id, CancellationToken.None).Returns(owner);
        // IsAdmin returns false and OwnerId returns Guid.Empty by default (NSubstitute)

        await Should.ThrowAsync<NotFoundException>(
            () => _getPetByIdQueryHandler.Handle(new GetPetByIdQuery(petId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_OwnerIsInvisible_And_CallerIsOwner_Returns_MappedDto()
    {
        var petId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        owner.SetIsVisible(false);
        var pet = new PetBuilder().WithOwnerId(owner.Id).WithName("Luna").Build();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);
        _ownerRepository.GetByIdAsync(owner.Id, CancellationToken.None).Returns(owner);
        _currentUserService.OwnerId.Returns((Guid?)owner.Id);

        var result = await _getPetByIdQueryHandler.Handle(new GetPetByIdQuery(petId), CancellationToken.None);

        result.Name.ShouldBe("Luna");
    }

    [Fact]
    public async Task Handle_When_OwnerIsInvisible_And_CallerIsAdmin_Returns_MappedDto()
    {
        var petId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        owner.SetIsVisible(false);
        var pet = new PetBuilder().WithOwnerId(owner.Id).WithName("Luna").Build();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);
        _ownerRepository.GetByIdAsync(owner.Id, CancellationToken.None).Returns(owner);
        _currentUserService.IsAdmin.Returns(true);

        var result = await _getPetByIdQueryHandler.Handle(new GetPetByIdQuery(petId), CancellationToken.None);

        result.Name.ShouldBe("Luna");
    }
}
