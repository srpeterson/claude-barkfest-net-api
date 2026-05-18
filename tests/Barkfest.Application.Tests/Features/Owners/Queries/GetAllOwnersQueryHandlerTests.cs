using Barkfest.Application.Features.Owners.Queries.GetAllOwners;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Owners.Queries;

public class GetAllOwnersQueryHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly GetAllOwnersQueryHandler _getAllOwnersQueryHandler;

    public GetAllOwnersQueryHandlerTests()
    {
        _getAllOwnersQueryHandler = new GetAllOwnersQueryHandler(_ownerRepository);
    }

    [Fact]
    public async Task Handle_When_OwnersExist_Returns_AllOwnersMappedToDtos()
    {
        var owners = new[]
        {
            new OwnerBuilder().WithFirstName("Alice").WithLastName("Adams").Build(),
            new OwnerBuilder().WithFirstName("Bob").WithLastName("Baker").Build()
        };
        _ownerRepository.GetAllAsync(CancellationToken.None).Returns(owners);

        var result = await _getAllOwnersQueryHandler.Handle(new GetAllOwnersQuery(), CancellationToken.None);

        var list = result.ToList();
        list.Count.ShouldBe(2);
        list[0].FirstName.ShouldBe("Alice");
        list[1].FirstName.ShouldBe("Bob");
    }

    [Fact]
    public async Task Handle_When_NoOwners_Returns_EmptyCollection()
    {
        _ownerRepository.GetAllAsync(CancellationToken.None).Returns([]);

        var result = await _getAllOwnersQueryHandler.Handle(new GetAllOwnersQuery(), CancellationToken.None);

        result.ShouldBeEmpty();
    }

}
