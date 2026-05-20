using Barkfest.Domain.Entities;
using Barkfest.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Barkfest.Persistence.Tests.Repositories;

public class OwnerRepositoryTests(DatabaseFixture fixture)
    : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private AppDbContext _context = null!;
    private IDbContextTransaction _transaction = null!;
    private OwnerRepository _ownerRepository = null!;

    public async Task InitializeAsync()
    {
        _context = fixture.CreateContext();
        _transaction = await _context.Database.BeginTransactionAsync();
        _ownerRepository = new OwnerRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_When_OwnerAdded_Returns_SavedOwner()
    {
        var owner = BuildOwner("johndoe", "John", "Doe", "john@example.com");
        await _ownerRepository.AddAsync(owner);
        await _context.SaveChangesAsync();

        var result = await _ownerRepository.GetByIdAsync(owner.Id);

        result.ShouldNotBeNull();
        result.FirstName.ShouldBe("John");
        result.LastName.ShouldBe("Doe");
        result.Email.ShouldBe("john@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_When_OwnerNotFound_Returns_Null()
    {
        var result = await _ownerRepository.GetByIdAsync(Guid.NewGuid());

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_When_OwnerExists_Returns_Owner()
    {
        var owner = BuildOwner("userlookup", "John", "Doe", "john-u@example.com");
        await _ownerRepository.AddAsync(owner);
        await _context.SaveChangesAsync();

        var result = await _ownerRepository.GetByUsernameAsync("userlookup");

        result.ShouldNotBeNull();
        result.Id.ShouldBe(owner.Id);
    }

    [Fact]
    public async Task GetByUsernameAsync_When_UsernameNotFound_Returns_Null()
    {
        var result = await _ownerRepository.GetByUsernameAsync("nobody");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAllAsync_When_Called_Returns_AllPersistedOwners()
    {
        await _ownerRepository.AddAsync(BuildOwner("aliceadams", "Alice", "Adams", "alice@example.com"));
        await _ownerRepository.AddAsync(BuildOwner("bobbaker", "Bob", "Baker", "bob@example.com"));
        await _context.SaveChangesAsync();

        var result = await _ownerRepository.GetAllAsync();

        result.Count().ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task UpdateAsync_When_OwnerUpdated_Persists_Changes()
    {
        var owner = BuildOwner("janesmith", "Jane", "Smith", "jane@example.com");
        await _ownerRepository.AddAsync(owner);
        await _context.SaveChangesAsync();

        owner.SetFirstName("Janet");
        owner.SetEmail("janet@example.com");
        await _ownerRepository.UpdateAsync(owner);
        await _context.SaveChangesAsync();

        var updated = await _context.Owners.AsNoTracking()
            .FirstAsync(o => o.Id == owner.Id);
        updated.FirstName.ShouldBe("Janet");
        updated.Email.ShouldBe("janet@example.com");
    }

    [Fact]
    public async Task DeleteAsync_When_OwnerExists_Removes_Owner()
    {
        var owner = BuildOwner("markjones", "Mark", "Jones", "mark@example.com");
        await _ownerRepository.AddAsync(owner);
        await _context.SaveChangesAsync();

        await _ownerRepository.DeleteAsync(owner.Id);
        await _context.SaveChangesAsync();

        var result = await _ownerRepository.GetByIdAsync(owner.Id);
        result.ShouldBeNull();
    }

    [Fact]
    public async Task AddAsync_When_OwnerHasProfileImage_Persists_Image()
    {
        var owner = BuildOwner("saralee", "Sara", "Lee", "sara@example.com");
        owner.SetProfileImage("owners/abc/photo.jpg", "image/jpeg");
        await _ownerRepository.AddAsync(owner);
        await _context.SaveChangesAsync();

        var result = await _ownerRepository.GetByIdAsync(owner.Id);

        result!.ProfileImage.ShouldNotBeNull();
        result.ProfileImage.BlobName.ShouldBe("owners/abc/photo.jpg");
        result.ProfileImage.ContentType.ShouldBe("image/jpeg");
    }

    private static Owner BuildOwner(string username, string firstName, string lastName, string email)
    {
        var owner = new Owner();
        owner.SetUsername(username);
        owner.SetFirstName(firstName);
        owner.SetLastName(lastName);
        owner.SetEmail(email);
        return owner;
    }
}
