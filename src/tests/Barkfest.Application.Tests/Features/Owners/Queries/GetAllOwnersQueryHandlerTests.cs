using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Owners.Queries.GetAllOwners;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Owners.Queries;

public class GetAllOwnersQueryHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly GetAllOwnersQueryHandler _getAllOwnersQueryHandler;

    public GetAllOwnersQueryHandlerTests()
    {
        _currentUserService.IsAdmin.Returns(true);
        _getAllOwnersQueryHandler = new GetAllOwnersQueryHandler(_ownerRepository, _currentUserService);
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

        result.IsSuccess.ShouldBeTrue();
        var list = result.Value.ToList();
        list.Count.ShouldBe(2);
        list[0].FirstName.ShouldBe("Alice");
        list[1].FirstName.ShouldBe("Bob");
    }

    [Fact]
    public async Task Handle_When_NoOwners_Returns_EmptyCollection()
    {
        _ownerRepository.GetAllAsync(CancellationToken.None).Returns([]);

        var result = await _getAllOwnersQueryHandler.Handle(new GetAllOwnersQuery(), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_When_NotAdmin_Returns_ForbiddenError()
    {
        _currentUserService.IsAdmin.Returns(false);

        var result = await _getAllOwnersQueryHandler.Handle(new GetAllOwnersQuery(), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ForbiddenError>();
    }
}
