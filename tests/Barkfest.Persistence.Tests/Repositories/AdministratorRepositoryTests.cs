using Barkfest.Domain.Entities;
using Barkfest.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Barkfest.Persistence.Tests.Repositories;

public class AdministratorRepositoryTests(DatabaseFixture fixture)
    : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private AppDbContext _context = null!;
    private IDbContextTransaction _transaction = null!;
    private AdministratorRepository _administratorRepository = null!;

    public async Task InitializeAsync()
    {
        _context = fixture.CreateContext();
        _transaction = await _context.Database.BeginTransactionAsync();
        _administratorRepository = new AdministratorRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_When_AdministratorAdded_Returns_SavedAdministrator()
    {
        var administrator = BuildAdministrator("persistadmin", "persist@barkfest.dev");
        await _administratorRepository.AddAsync(administrator);
        await _context.SaveChangesAsync();

        var result = await _administratorRepository.GetByIdAsync(administrator.Id);

        result.ShouldNotBeNull();
        result.Username.ShouldBe("persistadmin");
        result.Email.ShouldBe("persist@barkfest.dev");
        result.PasswordHash.ShouldBe("$2a$11$somehash");
    }

    [Fact]
    public async Task GetByIdAsync_When_AdministratorNotFound_Returns_Null()
    {
        var result = await _administratorRepository.GetByIdAsync(Guid.NewGuid());

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_When_AdministratorExists_Returns_Administrator()
    {
        var administrator = BuildAdministrator("lookupuser", "username-lookup@barkfest.dev");
        await _administratorRepository.AddAsync(administrator);
        await _context.SaveChangesAsync();

        var result = await _administratorRepository.GetByUsernameAsync("lookupuser");

        result.ShouldNotBeNull();
        result.Id.ShouldBe(administrator.Id);
    }

    [Fact]
    public async Task GetByUsernameAsync_When_UsernameNotFound_Returns_Null()
    {
        var result = await _administratorRepository.GetByUsernameAsync("ghostuser");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_When_AdministratorExists_Returns_Administrator()
    {
        var administrator = BuildAdministrator("emaillookup", "email-lookup@barkfest.dev");
        await _administratorRepository.AddAsync(administrator);
        await _context.SaveChangesAsync();

        var result = await _administratorRepository.GetByEmailAsync("email-lookup@barkfest.dev");

        result.ShouldNotBeNull();
        result.Id.ShouldBe(administrator.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_When_EmailNotFound_Returns_Null()
    {
        var result = await _administratorRepository.GetByEmailAsync("nothere@barkfest.dev");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_When_EmailHasDifferentCase_Returns_Administrator()
    {
        var administrator = BuildAdministrator("casecheck", "case-check@barkfest.dev");
        await _administratorRepository.AddAsync(administrator);
        await _context.SaveChangesAsync();

        var result = await _administratorRepository.GetByEmailAsync("CASE-CHECK@BARKFEST.DEV");

        result.ShouldNotBeNull();
        result.Id.ShouldBe(administrator.Id);
    }

    [Fact]
    public async Task DeleteAsync_When_AdministratorExists_Removes_Administrator()
    {
        var administrator = BuildAdministrator("deleteuser", "todelete@barkfest.dev");
        await _administratorRepository.AddAsync(administrator);
        await _context.SaveChangesAsync();

        await _administratorRepository.DeleteAsync(administrator);
        await _context.SaveChangesAsync();

        var result = await _administratorRepository.GetByIdAsync(administrator.Id);
        result.ShouldBeNull();
    }

    private static Administrator BuildAdministrator(string username, string email)
    {
        var administrator = new Administrator();
        administrator.SetUsername(username);
        administrator.SetName("Test Administrator");
        administrator.SetEmail(email);
        administrator.SetPhoneNumber("+15555550100");
        administrator.SetPasswordHash("$2a$11$somehash");
        return administrator;
    }
}
