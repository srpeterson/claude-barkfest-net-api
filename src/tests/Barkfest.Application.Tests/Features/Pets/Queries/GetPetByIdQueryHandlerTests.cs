using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Pets.Queries.GetPetById;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Queries;

public class GetPetByIdQueryHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly GetPetByIdQueryHandler _getPetByIdQueryHandler;

    public GetPetByIdQueryHandlerTests()
    {
        _getPetByIdQueryHandler = new GetPetByIdQueryHandler(_petRepository, _currentUserService);
    }

    [Fact]
    public async Task Handle_When_PetExists_Returns_MappedDto()
    {
        var petId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        var pet = new PetBuilder().WithOwner(owner).Build();
        _petRepository.GetByIdWithOwnerAsync(petId, CancellationToken.None).Returns(pet);

        var result = await _getPetByIdQueryHandler.Handle(new GetPetByIdQuery(petId), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe("Buddy");
        result.Value.PetType.ShouldBe("Dog");
    }

    [Fact]
    public async Task Handle_When_PetNotFound_Returns_NotFoundError()
    {
        var petId = Guid.NewGuid();
        _petRepository.GetByIdWithOwnerAsync(petId, CancellationToken.None).Returns((Pet?)null);

        var result = await _getPetByIdQueryHandler.Handle(new GetPetByIdQuery(petId), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<NotFoundError>();
    }

    [Fact]
    public async Task Handle_When_OwnerIsInactive_Returns_NotFoundError()
    {
        var petId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        owner.SetIsActive(false);
        var pet = new PetBuilder().WithOwner(owner).Build();
        _petRepository.GetByIdWithOwnerAsync(petId, CancellationToken.None).Returns(pet);
        // IsAdmin returns false by default (NSubstitute default for bool)

        var result = await _getPetByIdQueryHandler.Handle(new GetPetByIdQuery(petId), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<NotFoundError>();
    }

    [Fact]
    public async Task Handle_When_OwnerIsInactive_And_CallerIsAdmin_Returns_MappedDto()
    {
        var petId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        owner.SetIsActive(false);
        var pet = new PetBuilder().WithOwner(owner).WithName("Rex").Build();
        _petRepository.GetByIdWithOwnerAsync(petId, CancellationToken.None).Returns(pet);
        _currentUserService.IsAdmin.Returns(true);

        var result = await _getPetByIdQueryHandler.Handle(new GetPetByIdQuery(petId), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe("Rex");
    }

    [Fact]
    public async Task Handle_When_OwnerIsInvisible_Returns_NotFoundError()
    {
        var petId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        owner.SetIsVisible(false);
        var pet = new PetBuilder().WithOwner(owner).Build();
        _petRepository.GetByIdWithOwnerAsync(petId, CancellationToken.None).Returns(pet);
        // IsAdmin returns false and OwnerId returns null by default (NSubstitute)

        var result = await _getPetByIdQueryHandler.Handle(new GetPetByIdQuery(petId), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<NotFoundError>();
    }

    [Fact]
    public async Task Handle_When_OwnerIsInvisible_And_CallerIsOwner_Returns_MappedDto()
    {
        var petId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        owner.SetIsVisible(false);
        var pet = new PetBuilder().WithOwner(owner).WithName("Luna").Build();
        _petRepository.GetByIdWithOwnerAsync(petId, CancellationToken.None).Returns(pet);
        _currentUserService.OwnerId.Returns((Guid?)owner.Id);

        var result = await _getPetByIdQueryHandler.Handle(new GetPetByIdQuery(petId), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe("Luna");
    }

    [Fact]
    public async Task Handle_When_OwnerIsInvisible_And_CallerIsAdmin_Returns_MappedDto()
    {
        var petId = Guid.NewGuid();
        var owner = new OwnerBuilder().Build();
        owner.SetIsVisible(false);
        var pet = new PetBuilder().WithOwner(owner).WithName("Luna").Build();
        _petRepository.GetByIdWithOwnerAsync(petId, CancellationToken.None).Returns(pet);
        _currentUserService.IsAdmin.Returns(true);

        var result = await _getPetByIdQueryHandler.Handle(new GetPetByIdQuery(petId), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe("Luna");
    }
}
