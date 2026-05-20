using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barkfest.Persistence.Configurations;

public class BreedConfiguration : IEntityTypeConfiguration<Breed>
{
    public void Configure(EntityTypeBuilder<Breed> builder)
    {
        builder.ToTable("Breeds");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .HasColumnName("BreedId")
            .HasDefaultValueSql("newsequentialid()");

        builder.Property(b => b.PetId)
            .HasColumnName("PetId");

        builder.HasDiscriminator<string>("BreedType")
            .HasValue<DogBreedInfo>("Dog")
            .HasValue<CatBreedInfo>("Cat");

        builder.Property("BreedType")
            .HasMaxLength(50);

        builder.HasOne(b => b.Pet)
            .WithOne(p => p.Breed)
            .HasForeignKey<Breed>(b => b.PetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DogBreedInfoConfiguration : IEntityTypeConfiguration<DogBreedInfo>
{
    public void Configure(EntityTypeBuilder<DogBreedInfo> builder)
    {
        builder.Property(d => d.DogBreed)
            .HasColumnName("BreedValue")
            .HasConversion(
                v => v.Value,
                v => DogBreed.FromValue(v));
    }
}

public class CatBreedInfoConfiguration : IEntityTypeConfiguration<CatBreedInfo>
{
    public void Configure(EntityTypeBuilder<CatBreedInfo> builder)
    {
        builder.Property(c => c.CatBreed)
            .HasColumnName("BreedValue")
            .HasConversion(
                v => v.Value,
                v => CatBreed.FromValue(v));
    }
}
