using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using Barkfest.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Barkfest.Persistence.Tests.Repositories;

// Regression guard for the atomic like counter. Unlike the other repository tests,
// this one commits its seed data (no ambient transaction) so that each parallel
// connection sees the same row, then fires concurrent increments to prove no
// updates are lost. A read-modify-write implementation would drop updates here.
public class PetLikesConcurrencyTests(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task IncrementLikesAsync_When_CalledConcurrently_Returns_NoLostUpdates()
    {
        const int concurrentLikes = 50;

        var (ownerId, petId) = await SeedCommittedPet();

        try
        {
            var tasks = Enumerable.Range(0, concurrentLikes).Select(async _ =>
            {
                await using var ctx = fixture.CreateContext();
                var repo = new PetRepository(ctx);
                await repo.IncrementLikesAsync(petId);
            });

            await Task.WhenAll(tasks);

            await using var verify = fixture.CreateContext();
            var likes = await verify.Pets
                .Where(p => p.Id == petId)
                .Select(p => p.Likes)
                .FirstAsync();

            likes.ShouldBe(concurrentLikes);
        }
        finally
        {
            await using var cleanup = fixture.CreateContext();
            await cleanup.Pets.Where(p => p.Id == petId).ExecuteDeleteAsync();
            await cleanup.Owners.Where(o => o.Id == ownerId).ExecuteDeleteAsync();
        }
    }

    private async Task<(Guid ownerId, Guid petId)> SeedCommittedPet()
    {
        await using var seed = fixture.CreateContext();

        var owner = new Owner();
        owner.SetUsername($"u{Guid.NewGuid():N}");
        owner.SetFirstName("Test");
        owner.SetLastName("Owner");
        owner.SetEmail($"{Guid.NewGuid():N}@example.com");

        var pet = Pet.Create(owner.Id, "Buddy", PetType.Dog, DogBreed.Beagle.Value);

        seed.Owners.Add(owner);
        seed.Pets.Add(pet);
        await seed.SaveChangesAsync();

        return (owner.Id, pet.Id);
    }
}
