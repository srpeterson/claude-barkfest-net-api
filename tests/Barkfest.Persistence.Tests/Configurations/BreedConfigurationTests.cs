using Barkfest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Barkfest.Persistence.Tests.Configurations;

public class BreedConfigurationTests
{
    private readonly IEntityType _breed =
        ModelHelper.Model.FindEntityType(typeof(Breed))!;

    private readonly IEntityType _dogBreed =
        ModelHelper.Model.FindEntityType(typeof(DogBreedInfo))!;

    private readonly IEntityType _catBreed =
        ModelHelper.Model.FindEntityType(typeof(CatBreedInfo))!;

    private StoreObjectIdentifier TableId =>
        StoreObjectIdentifier.Table(_breed.GetTableName()!, _breed.GetSchema());

    // -----------------------------------------------------------------------
    // Table (TPH — all types share one table)
    // -----------------------------------------------------------------------

    [Fact]
    public void Breed_MapsToBreedsTable()
    {
        _breed.GetTableName().ShouldBe("Breeds");
    }

    [Fact]
    public void DogBreedInfo_SharesBreedsTable()
    {
        _dogBreed.GetTableName().ShouldBe("Breeds");
    }

    [Fact]
    public void CatBreedInfo_SharesBreedsTable()
    {
        _catBreed.GetTableName().ShouldBe("Breeds");
    }

    // -----------------------------------------------------------------------
    // Primary key
    // -----------------------------------------------------------------------

    [Fact]
    public void Id_MapsToBreedIdColumn()
    {
        _breed.FindProperty(nameof(Breed.Id))!
              .GetColumnName(TableId)
              .ShouldBe("BreedId");
    }

    [Fact]
    public void Id_HasDatabaseGeneratedDefault()
    {
        _breed.FindProperty(nameof(Breed.Id))!
              .GetDefaultValueSql()
              .ShouldBe("newsequentialid()");
    }

    // -----------------------------------------------------------------------
    // TPH discriminator
    // -----------------------------------------------------------------------

    [Fact]
    public void Discriminator_ColumnIsBreedType()
    {
        var discriminator = _breed.FindDiscriminatorProperty();
        discriminator.ShouldNotBeNull();
        discriminator!.GetColumnName(TableId).ShouldBe("BreedType");
    }

    [Fact]
    public void Discriminator_HasMaxLength50()
    {
        _breed.FindDiscriminatorProperty()!
              .GetMaxLength()
              .ShouldBe(50);
    }

    [Fact]
    public void DogBreedInfo_HasDiscriminatorValueDog()
    {
        _dogBreed.GetDiscriminatorValue().ShouldBe("Dog");
    }

    [Fact]
    public void CatBreedInfo_HasDiscriminatorValueCat()
    {
        _catBreed.GetDiscriminatorValue().ShouldBe("Cat");
    }

    // -----------------------------------------------------------------------
    // BreedValue column (shared by Dog and Cat via TPH)
    // -----------------------------------------------------------------------

    [Fact]
    public void DogBreed_MapsToBreedValueColumn()
    {
        _dogBreed.FindProperty(nameof(DogBreedInfo.DogBreed))!
                 .GetColumnName(TableId)
                 .ShouldBe("BreedValue");
    }

    [Fact]
    public void CatBreed_MapsToBreedValueColumn()
    {
        _catBreed.FindProperty(nameof(CatBreedInfo.CatBreed))!
                 .GetColumnName(TableId)
                 .ShouldBe("BreedValue");
    }

    // -----------------------------------------------------------------------
    // Pet FK (one-to-one, cascade delete)
    // -----------------------------------------------------------------------

    [Fact]
    public void Breed_HasForeignKeyToPet()
    {
        var fk = _breed.GetForeignKeys()
                       .SingleOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Pet));

        fk.ShouldNotBeNull();
    }

    [Fact]
    public void Breed_PetForeignKey_IsCascadeDelete()
    {
        var fk = _breed.GetForeignKeys()
                       .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Pet));

        fk.DeleteBehavior.ShouldBe(DeleteBehavior.Cascade);
    }

    [Fact]
    public void Breed_PetForeignKey_UsesPetIdColumn()
    {
        var fk = _breed.GetForeignKeys()
                       .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Pet));

        fk.Properties.ShouldContain(p => p.GetColumnName(TableId) == "PetId");
    }
}
