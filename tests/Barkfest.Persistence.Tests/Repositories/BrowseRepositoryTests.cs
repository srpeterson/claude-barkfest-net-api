using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using Barkfest.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Barkfest.Persistence.Tests.Repositories;

public class BrowseRepositoryTests(DatabaseFixture fixture)
    : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private AppDbContext _context = null!;
    private IDbContextTransaction _transaction = null!;
    private BrowseRepository _browseRepository = null!;

    public async Task InitializeAsync()
    {
        _context = fixture.CreateContext();
        _transaction = await _context.Database.BeginTransactionAsync();
        _browseRepository = new BrowseRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _context.DisposeAsync();
    }

    // -----------------------------------------------------------------------
    // Ordering
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetBrowseImagesAsync_When_MultiplePets_Returns_NewestPetFirst()
    {
        var owner = await SeedOwnerAsync();

        var olderPet = BuildPetWithFeaturedImage(owner.Id, "OlderPet");
        _context.Pets.Add(olderPet);
        await _context.SaveChangesAsync();

        // Delay ensures Pet.CreatedAt differs — this is intentional, not a poll
        await Task.Delay(50);

        var newerPet = BuildPetWithFeaturedImage(owner.Id, "NewerPet");
        _context.Pets.Add(newerPet);
        await _context.SaveChangesAsync();

        var result = await _browseRepository.GetBrowseImagesAsync(
            null, null, page: 1, pageSize: 10, CancellationToken.None);

        result.Items.Count.ShouldBe(2);
        result.Items[0].PetName.ShouldBe("NewerPet");
        result.Items[1].PetName.ShouldBe("OlderPet");
    }

    // -----------------------------------------------------------------------
    // Featured image filter
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetBrowseImagesAsync_When_PetHasMultipleImages_Returns_OnlyFeaturedImage()
    {
        var owner = await SeedOwnerAsync();
        var pet = BuildPet(owner.Id, "Buddy");

        var featured    = BuildImage("pets/1/gallery/featured.jpg",  isFeatured: true);
        var nonFeatured = BuildImage("pets/1/gallery/extra.jpg",     isFeatured: false);
        pet.AddImage(featured);

        // AddImage sets the first image as featured — manually add second without using AddImage
        // so we can control IsFeaturedImage independently
        nonFeatured.UnsetAsFeatured();
        _context.Pets.Add(pet);
        _context.Set<PetImage>().Add(nonFeatured);

        // Wire nonFeatured to pet via PetId using shadow property
        _context.Entry(nonFeatured).Property("PetId").CurrentValue = pet.Id;

        await _context.SaveChangesAsync();

        var result = await _browseRepository.GetBrowseImagesAsync(
            null, null, page: 1, pageSize: 10, CancellationToken.None);

        var petImages = result.Items.Where(i => i.PetId == pet.Id).ToList();
        petImages.Count.ShouldBe(1);
        petImages[0].IsFeaturedImage.ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // Pagination
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetBrowseImagesAsync_When_PageSizeIsOne_Returns_OneItem_WithCorrectTotalCount()
    {
        var owner = await SeedOwnerAsync();
        _context.Pets.Add(BuildPetWithFeaturedImage(owner.Id, "Pet1"));
        _context.Pets.Add(BuildPetWithFeaturedImage(owner.Id, "Pet2"));
        _context.Pets.Add(BuildPetWithFeaturedImage(owner.Id, "Pet3"));
        await _context.SaveChangesAsync();

        var result = await _browseRepository.GetBrowseImagesAsync(
            null, null, page: 1, pageSize: 1, CancellationToken.None);

        result.Items.Count.ShouldBe(1);
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(3);
        result.HasMore.ShouldBeTrue();
    }

    [Fact]
    public async Task GetBrowseImagesAsync_When_OnLastPage_HasMore_IsFalse()
    {
        var owner = await SeedOwnerAsync();
        _context.Pets.Add(BuildPetWithFeaturedImage(owner.Id, "OnlyPet"));
        await _context.SaveChangesAsync();

        var result = await _browseRepository.GetBrowseImagesAsync(
            null, null, page: 1, pageSize: 100, CancellationToken.None);

        result.HasMore.ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // Pet type filter
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetBrowseImagesAsync_When_PetTypeFilterApplied_Returns_OnlyMatchingType()
    {
        var owner = await SeedOwnerAsync();
        _context.Pets.Add(BuildPetWithFeaturedImage(owner.Id, "DogPet", PetType.Dog, BuildDogBreed(DogBreed.Beagle)));
        _context.Pets.Add(BuildPetWithFeaturedImage(owner.Id, "CatPet", PetType.Cat, BuildCatBreed(CatBreed.Persian)));
        await _context.SaveChangesAsync();

        var result = await _browseRepository.GetBrowseImagesAsync(
            PetType.Dog, null, page: 1, pageSize: 10, CancellationToken.None);

        result.Items.Any(i => i.PetName == "DogPet").ShouldBeTrue();
        result.Items.Any(i => i.PetName == "CatPet").ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // Breed filter
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetBrowseImagesAsync_When_BreedFilterApplied_Returns_OnlyMatchingBreed()
    {
        var owner = await SeedOwnerAsync();
        _context.Pets.Add(BuildPetWithFeaturedImage(owner.Id, "BeaglePet",    PetType.Dog, BuildDogBreed(DogBreed.Beagle)));
        _context.Pets.Add(BuildPetWithFeaturedImage(owner.Id, "LabradorPet",  PetType.Dog, BuildDogBreed(DogBreed.LabradorRetriever)));
        await _context.SaveChangesAsync();

        var result = await _browseRepository.GetBrowseImagesAsync(
            null, "Beagle", page: 1, pageSize: 10, CancellationToken.None);

        result.Items.Any(i => i.PetName == "BeaglePet").ShouldBeTrue();
        result.Items.Any(i => i.PetName == "LabradorPet").ShouldBeFalse();
    }

    [Fact]
    public async Task GetBrowseImagesAsync_When_BreedIsUnrecognised_Returns_EmptyResult()
    {
        var owner = await SeedOwnerAsync();
        _context.Pets.Add(BuildPetWithFeaturedImage(owner.Id, "Buddy"));
        await _context.SaveChangesAsync();

        var result = await _browseRepository.GetBrowseImagesAsync(
            null, "not-a-real-breed", page: 1, pageSize: 10, CancellationToken.None);

        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private async Task<Owner> SeedOwnerAsync()
    {
        var ownerRepo = new OwnerRepository(_context);
        var owner = new Owner();
        owner.SetUsername($"u{Guid.NewGuid():N}");
        owner.SetFirstName("Test");
        owner.SetLastName("Owner");
        owner.SetEmail($"browse-{Guid.NewGuid():N}@example.com");
        await ownerRepo.AddAsync(owner);
        await _context.SaveChangesAsync();
        return owner;
    }

    private static Pet BuildPetWithFeaturedImage(Guid ownerId, string name)
        => BuildPetWithFeaturedImage(ownerId, name, PetType.Dog, BuildDogBreed(DogBreed.Beagle));

    private static Pet BuildPetWithFeaturedImage(Guid ownerId, string name, PetType petType, Breed breed)
    {
        var pet   = Pet.Create(ownerId, name, petType, breed);
        var image = BuildImage($"pets/{ownerId}/gallery/{Guid.NewGuid()}.jpg", isFeatured: true);
        pet.AddImage(image);
        return pet;
    }

    private static Pet BuildPet(Guid ownerId, string name)
        => Pet.Create(ownerId, name, PetType.Dog, BuildDogBreed(DogBreed.Beagle));

    private static DogBreedInfo BuildDogBreed(DogBreed dogBreed)
    {
        var breed = new DogBreedInfo();
        breed.SetDogBreed(dogBreed);
        return breed;
    }

    private static CatBreedInfo BuildCatBreed(CatBreed catBreed)
    {
        var breed = new CatBreedInfo();
        breed.SetCatBreed(catBreed);
        return breed;
    }

    private static PetImage BuildImage(string blobName, bool isFeatured)
    {
        var image = new PetImage();
        image.SetImage(blobName, "image/jpeg");
        image.SetDisplayOrder(0);
        if (!isFeatured) image.UnsetAsFeatured();
        return image;
    }
}
