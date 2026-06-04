using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Administrators.Queries.GetAllAdministrators;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Administrators.Queries;

public class GetAllAdministratorsQueryHandlerTests
{
    private readonly IAdministratorRepository _administratorRepository = Substitute.For<IAdministratorRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly GetAllAdministratorsQueryHandler _getAllAdministratorsQueryHandler;

    public GetAllAdministratorsQueryHandlerTests()
    {
        _currentUserService.IsAdmin.Returns(true);
        _getAllAdministratorsQueryHandler = new GetAllAdministratorsQueryHandler(
            _administratorRepository, _currentUserService);
    }

    [Fact]
    public async Task Handle_When_AdministratorsExist_Returns_AllAdministratorsMappedToDtos()
    {
        var admin1 = new Administrator();
        admin1.SetUsername("alice");
        admin1.SetName("Alice Adams");
        admin1.SetEmail("alice@barkfest.dev");
        admin1.SetPhoneNumber("+15555550101");
        admin1.SetPasswordHash("hash1");

        var admin2 = new Administrator();
        admin2.SetUsername("bob");
        admin2.SetName("Bob Baker");
        admin2.SetEmail("bob@barkfest.dev");
        admin2.SetPhoneNumber("+15555550102");
        admin2.SetPasswordHash("hash2");

        _administratorRepository.GetAllAsync(CancellationToken.None).Returns([admin1, admin2]);

        var result = await _getAllAdministratorsQueryHandler.Handle(
            new GetAllAdministratorsQuery(), CancellationToken.None);

        var list = result.ToList();
        list.Count.ShouldBe(2);
        list[0].Username.ShouldBe("alice");
        list[1].Username.ShouldBe("bob");
    }

    [Fact]
    public async Task Handle_When_NoAdministrators_Returns_EmptyCollection()
    {
        _administratorRepository.GetAllAsync(CancellationToken.None).Returns([]);

        var result = await _getAllAdministratorsQueryHandler.Handle(
            new GetAllAdministratorsQuery(), CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_When_NotAdmin_Throws_ForbiddenException()
    {
        _currentUserService.IsAdmin.Returns(false);

        var act = () => _getAllAdministratorsQueryHandler.Handle(
            new GetAllAdministratorsQuery(), CancellationToken.None);

        await act.ShouldThrowAsync<ForbiddenException>();
    }
}
