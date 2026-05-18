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
    private OwnerRepository _sut = null!;

    public async Task InitializeAsync()
    {
        _context = fixture.CreateContext();
        _transaction = await _context.Database.BeginTransactionAsync();
        _sut = new OwnerRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_When_OwnerAdded_Returns_SavedOwner()
    {
        var owner = BuildOwner("John", "Doe", "john@example.com");
        await _sut.AddAsync(owner);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(owner.Id);

        result.ShouldNotBeNull();
        result.FirstName.ShouldBe("John");
        result.LastName.ShouldBe("Doe");
        result.Email.ShouldBe("john@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_When_OwnerNotFound_Returns_Null()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAllAsync_When_Called_Returns_AllPersistedOwners()
    {
        await _sut.AddAsync(BuildOwner("Alice", "Adams", "alice@example.com"));
        await _sut.AddAsync(BuildOwner("Bob", "Baker", "bob@example.com"));
        await _context.SaveChangesAsync();

        var result = await _sut.GetAllAsync();

        result.Count().ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task UpdateAsync_When_OwnerUpdated_Persists_Changes()
    {
        var owner = BuildOwner("Jane", "Smith", "jane@example.com");
        await _sut.AddAsync(owner);
        await _context.SaveChangesAsync();

        owner.SetFirstName("Janet");
        owner.SetEmail("janet@example.com");
        await _sut.UpdateAsync(owner);
        await _context.SaveChangesAsync();

        var updated = await _context.Owners.AsNoTracking()
            .FirstAsync(o => o.Id == owner.Id);
        updated.FirstName.ShouldBe("Janet");
        updated.Email.ShouldBe("janet@example.com");
    }

    [Fact]
    public async Task DeleteAsync_When_OwnerExists_Removes_Owner()
    {
        var owner = BuildOwner("Mark", "Jones", "mark@example.com");
        await _sut.AddAsync(owner);
        await _context.SaveChangesAsync();

        await _sut.DeleteAsync(owner.Id);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(owner.Id);
        result.ShouldBeNull();
    }

    [Fact]
    public async Task AddAsync_When_OwnerHasProfileImage_Persists_Image()
    {
        var owner = BuildOwner("Sara", "Lee", "sara@example.com");
        owner.SetProfileImage("owners/abc/photo.jpg", "image/jpeg");
        await _sut.AddAsync(owner);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(owner.Id);

        result!.ProfileImage.ShouldNotBeNull();
        result.ProfileImage.BlobName.ShouldBe("owners/abc/photo.jpg");
        result.ProfileImage.ContentType.ShouldBe("image/jpeg");
    }

    private static Owner BuildOwner(string firstName, string lastName, string email)
    {
        var owner = new Owner();
        owner.SetFirstName(firstName);
        owner.SetLastName(lastName);
        owner.SetEmail(email);
        return owner;
    }
}
