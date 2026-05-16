using Barkfest.Application.Features.Owners.Queries.GetAllOwners;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Owners.Queries;

public class GetAllOwnersQueryHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly GetAllOwnersQueryHandler _sut;

    public GetAllOwnersQueryHandlerTests()
    {
        _sut = new GetAllOwnersQueryHandler(_ownerRepository);
    }

    [Fact]
    public async Task Handle_ReturnsAllOwnersMappedToDtos()
    {
        var owners = new[] { BuildOwner("Alice", "Adams"), BuildOwner("Bob", "Baker") };
        _ownerRepository.GetAllAsync(CancellationToken.None).Returns(owners);

        var result = await _sut.Handle(new GetAllOwnersQuery(), CancellationToken.None);

        var list = result.ToList();
        list.Count.ShouldBe(2);
        list[0].FirstName.ShouldBe("Alice");
        list[1].FirstName.ShouldBe("Bob");
    }

    [Fact]
    public async Task Handle_NoOwners_ReturnsEmptyCollection()
    {
        _ownerRepository.GetAllAsync(CancellationToken.None).Returns([]);

        var result = await _sut.Handle(new GetAllOwnersQuery(), CancellationToken.None);

        result.ShouldBeEmpty();
    }

    private static Owner BuildOwner(string firstName, string lastName)
    {
        var owner = new Owner();
        owner.SetFirstName(firstName);
        owner.SetLastName(lastName);
        owner.SetEmail($"{firstName.ToLower()}@example.com");
        return owner;
    }
}
