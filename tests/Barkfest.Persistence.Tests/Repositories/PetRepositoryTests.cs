using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using Barkfest.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Barkfest.Persistence.Tests.Repositories;

public class PetRepositoryTests(DatabaseFixture fixture)
    : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private AppDbContext _context = null!;
    private IDbContextTransaction _transaction = null!;
    private PetRepository _sut = null!;

    public async Task InitializeAsync()
    {
        _context = fixture.CreateContext();
        _transaction = await _context.Database.BeginTransactionAsync();
        _sut = new PetRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ReturnsSavedPet()
    {
        var (owner, _) = await SeedOwner();
        var pet = BuildPet(owner.Id, "Buddy", PetType.Dog);
        await _sut.AddAsync(pet);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(pet.Id);

        result.ShouldNotBeNull();
        result.Name.ShouldBe("Buddy");
        result.PetType.ShouldBe(PetType.Dog);
        result.OwnerId.ShouldBe(owner.Id);
    }

    [Fact]
    public async Task GetByIdAsync_PetNotFound_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllPersistedPets()
    {
        var (owner, _) = await SeedOwner();
        await _sut.AddAsync(BuildPet(owner.Id, "Max", PetType.Dog));
        await _sut.AddAsync(BuildPet(owner.Id, "Luna", PetType.Cat));
        await _context.SaveChangesAsync();

        var result = await _sut.GetAllAsync();

        result.Count().ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetByOwnerIdAsync_ReturnsOnlyPetsForThatOwner()
    {
        var (owner1, _) = await SeedOwner("alice@example.com");
        var (owner2, _) = await SeedOwner("bob@example.com");
        await _sut.AddAsync(BuildPet(owner1.Id, "Buddy", PetType.Dog));
        await _sut.AddAsync(BuildPet(owner1.Id, "Milo", PetType.Cat));
        await _sut.AddAsync(BuildPet(owner2.Id, "Shadow", PetType.Other));
        await _context.SaveChangesAsync();

        var result = await _sut.GetByOwnerIdAsync(owner1.Id);

        var list = result.ToList();
        list.Count.ShouldBe(2);
        list.ShouldAllBe(p => p.OwnerId == owner1.Id);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChangesToDatabase()
    {
        var (owner, _) = await SeedOwner();
        var pet = BuildPet(owner.Id, "Max", PetType.Dog);
        await _sut.AddAsync(pet);
        await _context.SaveChangesAsync();

        pet.SetName("Maxwell");
        pet.SetDescription("Very good boy");
        await _sut.UpdateAsync(pet);
        await _context.SaveChangesAsync();

        var updated = await _context.Pets.AsNoTracking()
            .FirstAsync(p => p.Id == pet.Id);
        updated.Name.ShouldBe("Maxwell");
        updated.Description.ShouldBe("Very good boy");
    }

    [Fact]
    public async Task DeleteAsync_RemovesPetFromDatabase()
    {
        var (owner, _) = await SeedOwner();
        var pet = BuildPet(owner.Id, "Rex", PetType.Dog);
        await _sut.AddAsync(pet);
        await _context.SaveChangesAsync();

        await _sut.DeleteAsync(pet.Id);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(pet.Id);
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_PetWithImages_IncludesImages()
    {
        var (owner, _) = await SeedOwner();
        var pet = BuildPet(owner.Id, "Daisy", PetType.Dog);
        var image = new PetImage();
        image.SetImage("pets/abc/gallery/photo.jpg", "image/jpeg");
        image.SetDisplayOrder(0);
        pet.AddImage(image);
        await _sut.AddAsync(pet);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(pet.Id);

        result!.Images.Count.ShouldBe(1);
        result.Images.First().BlobName.ShouldBe("pets/abc/gallery/photo.jpg");
    }

    [Fact]
    public async Task AddAsync_PetWithProfileImage_PersistsImage()
    {
        var (owner, _) = await SeedOwner();
        var pet = BuildPet(owner.Id, "Coco", PetType.Cat);
        pet.SetProfileImage("pets/abc/profile/photo.png", "image/png");
        await _sut.AddAsync(pet);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(pet.Id);

        result!.ProfileImage.ShouldNotBeNull();
        result.ProfileImage.BlobName.ShouldBe("pets/abc/profile/photo.png");
    }

    private async Task<(Owner owner, OwnerRepository repo)> SeedOwner(string email = "owner@example.com")
    {
        var ownerRepo = new OwnerRepository(_context);
        var owner = new Owner();
        owner.SetFirstName("Test");
        owner.SetLastName("Owner");
        owner.SetEmail(email);
        await ownerRepo.AddAsync(owner);
        await _context.SaveChangesAsync();
        return (owner, ownerRepo);
    }

    private static Pet BuildPet(Guid ownerId, string name, PetType petType)
    {
        var pet = new Pet(ownerId);
        pet.SetName(name);
        pet.SetPetType(petType);
        return pet;
    }
}
