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
    private PetRepository _petRepository = null!;

    public async Task InitializeAsync()
    {
        _context = fixture.CreateContext();
        _transaction = await _context.Database.BeginTransactionAsync();
        _petRepository = new PetRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_When_PetAdded_Returns_SavedPet()
    {
        var (owner, _) = await SeedOwner();
        var pet = BuildPet(owner.Id, "Buddy", PetType.Dog);
        await _petRepository.AddAsync(pet);
        await _context.SaveChangesAsync();

        var result = await _petRepository.GetByIdAsync(pet.Id);

        result.ShouldNotBeNull();
        result.Name.ShouldBe("Buddy");
        result.PetType.ShouldBe(PetType.Dog);
        result.OwnerId.ShouldBe(owner.Id);
    }

    [Fact]
    public async Task GetByIdAsync_When_PetNotFound_Returns_Null()
    {
        var result = await _petRepository.GetByIdAsync(Guid.NewGuid());

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAllAsync_When_Called_Returns_AllPersistedPets()
    {
        var (owner, _) = await SeedOwner();
        await _petRepository.AddAsync(BuildPet(owner.Id, "Max", PetType.Dog));
        await _petRepository.AddAsync(BuildPet(owner.Id, "Luna", PetType.Cat));
        await _context.SaveChangesAsync();

        var result = await _petRepository.GetAllAsync();

        result.Count().ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetByOwnerIdAsync_When_Called_Returns_OnlyPetsForThatOwner()
    {
        var (owner1, _) = await SeedOwner("alice@example.com");
        var (owner2, _) = await SeedOwner("bob@example.com");
        await _petRepository.AddAsync(BuildPet(owner1.Id, "Buddy", PetType.Dog));
        await _petRepository.AddAsync(BuildPet(owner1.Id, "Milo", PetType.Cat));
        await _petRepository.AddAsync(BuildPet(owner2.Id, "Shadow", PetType.Dog));
        await _context.SaveChangesAsync();

        var result = await _petRepository.GetByOwnerIdAsync(owner1.Id);

        var list = result.ToList();
        list.Count.ShouldBe(2);
        list.ShouldAllBe(p => p.OwnerId == owner1.Id);
    }

    [Fact]
    public async Task UpdateAsync_When_PetUpdated_Persists_Changes()
    {
        var (owner, _) = await SeedOwner();
        var pet = BuildPet(owner.Id, "Max", PetType.Dog);
        await _petRepository.AddAsync(pet);
        await _context.SaveChangesAsync();

        pet.SetName("Maxwell");
        pet.SetDescription("Very good boy");
        await _petRepository.UpdateAsync(pet);
        await _context.SaveChangesAsync();

        var updated = await _context.Pets.AsNoTracking()
            .FirstAsync(p => p.Id == pet.Id);
        updated.Name.ShouldBe("Maxwell");
        updated.Description.ShouldBe("Very good boy");
    }

    [Fact]
    public async Task DeleteAsync_When_PetExists_Removes_Pet()
    {
        var (owner, _) = await SeedOwner();
        var pet = BuildPet(owner.Id, "Rex", PetType.Dog);
        await _petRepository.AddAsync(pet);
        await _context.SaveChangesAsync();

        await _petRepository.DeleteAsync(pet.Id);
        await _context.SaveChangesAsync();

        var result = await _petRepository.GetByIdAsync(pet.Id);
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_When_PetHasImages_Returns_PetWithImages()
    {
        var (owner, _) = await SeedOwner();
        var pet = BuildPet(owner.Id, "Daisy", PetType.Dog);
        var image = new PetImage();
        image.SetImage("pets/abc/gallery/photo.jpg", "image/jpeg");
        image.SetDisplayOrder(0);
        pet.AddImage(image);
        await _petRepository.AddAsync(pet);
        await _context.SaveChangesAsync();

        var result = await _petRepository.GetByIdAsync(pet.Id);

        result!.Images.Count.ShouldBe(1);
        result.Images.First().BlobName.ShouldBe("pets/abc/gallery/photo.jpg");
    }

    [Fact]
    public async Task GetByIdWithOwnerAsync_When_PetExists_Returns_PetWithOwnerAndImages()
    {
        var (owner, _) = await SeedOwner();
        var pet = BuildPet(owner.Id, "Daisy", PetType.Dog);
        var image = new PetImage();
        image.SetImage("pets/abc/gallery/photo.jpg", "image/jpeg");
        image.SetDisplayOrder(0);
        pet.AddImage(image);
        await _petRepository.AddAsync(pet);
        await _context.SaveChangesAsync();

        var result = await _petRepository.GetByIdWithOwnerAsync(pet.Id);

        result.ShouldNotBeNull();
        result.Owner.ShouldNotBeNull();
        result.Owner.Id.ShouldBe(owner.Id);
        result.Images.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetByIdWithOwnerAsync_When_PetNotFound_Returns_Null()
    {
        var result = await _petRepository.GetByIdWithOwnerAsync(Guid.NewGuid());

        result.ShouldBeNull();
    }

    [Fact]
    public async Task IncrementLikesAsync_When_PetExists_Returns_NewCount()
    {
        var (owner, _) = await SeedOwner();
        var pet = BuildPet(owner.Id, "Buddy", PetType.Dog);
        await _petRepository.AddAsync(pet);
        await _context.SaveChangesAsync();

        var result = await _petRepository.IncrementLikesAsync(pet.Id);

        result.PetExists.ShouldBeTrue();
        result.Likes.ShouldBe(1);
        var stored = await _context.Pets.AsNoTracking().FirstAsync(p => p.Id == pet.Id);
        stored.Likes.ShouldBe(1);
    }

    [Fact]
    public async Task IncrementLikesAsync_When_PetNotFound_Returns_PetExistsFalse()
    {
        var result = await _petRepository.IncrementLikesAsync(Guid.NewGuid());

        result.PetExists.ShouldBeFalse();
    }

    [Fact]
    public async Task DecrementLikesAsync_When_LikesIsPositive_Returns_NewCount()
    {
        var (owner, _) = await SeedOwner();
        var pet = BuildPet(owner.Id, "Buddy", PetType.Dog);
        await _petRepository.AddAsync(pet);
        await _context.SaveChangesAsync();
        await _petRepository.IncrementLikesAsync(pet.Id);
        await _petRepository.IncrementLikesAsync(pet.Id);

        var result = await _petRepository.DecrementLikesAsync(pet.Id);

        result.PetExists.ShouldBeTrue();
        result.Likes.ShouldBe(1);
    }

    [Fact]
    public async Task DecrementLikesAsync_When_LikesIsZero_Returns_Zero()
    {
        var (owner, _) = await SeedOwner();
        var pet = BuildPet(owner.Id, "Buddy", PetType.Dog);
        await _petRepository.AddAsync(pet);
        await _context.SaveChangesAsync();

        var result = await _petRepository.DecrementLikesAsync(pet.Id);

        result.PetExists.ShouldBeTrue();
        result.Likes.ShouldBe(0);
    }

    [Fact]
    public async Task DecrementLikesAsync_When_PetNotFound_Returns_PetExistsFalse()
    {
        var result = await _petRepository.DecrementLikesAsync(Guid.NewGuid());

        result.PetExists.ShouldBeFalse();
    }

    private async Task<(Owner owner, OwnerRepository repo)> SeedOwner(string email = "owner@example.com")
    {
        var ownerRepo = new OwnerRepository(_context);
        var owner = new Owner();
        owner.SetUsername($"u{Guid.NewGuid():N}");
        owner.SetFirstName("Test");
        owner.SetLastName("Owner");
        owner.SetEmail(email);
        await ownerRepo.AddAsync(owner);
        await _context.SaveChangesAsync();
        return (owner, ownerRepo);
    }

    private static Pet BuildPet(Guid ownerId, string name, PetType petType)
    {
        var breedValue = petType == PetType.Dog
            ? DogBreed.Beagle.Value
            : CatBreed.Siamese.Value;

        return Pet.Create(ownerId, name, petType, breedValue);
    }
}
