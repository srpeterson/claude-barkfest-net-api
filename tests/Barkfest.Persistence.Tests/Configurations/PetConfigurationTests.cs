using Barkfest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Barkfest.Persistence.Tests.Configurations;

public class PetConfigurationTests
{
    private readonly IEntityType _pet =
        ModelHelper.Model.FindEntityType(typeof(Pet))!;

    private StoreObjectIdentifier TableId =>
        StoreObjectIdentifier.Table(_pet.GetTableName()!, _pet.GetSchema());

    // -----------------------------------------------------------------------
    // Table
    // -----------------------------------------------------------------------

    [Fact]
    public void Pet_MapsToPetsTable()
    {
        _pet.GetTableName().ShouldBe("Pets");
    }

    // -----------------------------------------------------------------------
    // Primary key
    // -----------------------------------------------------------------------

    [Fact]
    public void Id_MapsToPetIdColumn()
    {
        _pet.FindProperty(nameof(Pet.Id))!
            .GetColumnName(TableId)
            .ShouldBe("PetId");
    }

    [Fact]
    public void Id_HasDatabaseGeneratedDefault()
    {
        _pet.FindProperty(nameof(Pet.Id))!
            .GetDefaultValueSql()
            .ShouldBe("newsequentialid()");
    }

    // -----------------------------------------------------------------------
    // Name
    // -----------------------------------------------------------------------

    [Fact]
    public void Name_HasCorrectMaxLength()
    {
        _pet.FindProperty(nameof(Pet.Name))!
            .GetMaxLength()
            .ShouldBe(Pet.NameMaxLength);
    }

    [Fact]
    public void Name_IsRequired()
    {
        _pet.FindProperty(nameof(Pet.Name))!
            .IsNullable
            .ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // Description
    // -----------------------------------------------------------------------

    [Fact]
    public void Description_IsNullable()
    {
        _pet.FindProperty(nameof(Pet.Description))!
            .IsNullable
            .ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // DateOfBirth
    // -----------------------------------------------------------------------

    [Fact]
    public void DateOfBirth_IsNullable()
    {
        _pet.FindProperty(nameof(Pet.DateOfBirth))!
            .IsNullable
            .ShouldBeTrue();
    }

    [Fact]
    public void DateOfBirth_HasDateColumnType()
    {
        _pet.FindProperty(nameof(Pet.DateOfBirth))!
            .GetColumnType()
            .ShouldBe("date");
    }

    // -----------------------------------------------------------------------
    // PetType
    // -----------------------------------------------------------------------

    [Fact]
    public void PetType_IsRequired()
    {
        _pet.FindProperty(nameof(Pet.PetType))!
            .IsNullable
            .ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // Age (computed — must not be mapped to a column)
    // -----------------------------------------------------------------------

    [Fact]
    public void Age_IsNotMappedToAColumn()
    {
        _pet.FindProperty(nameof(Pet.Age)).ShouldBeNull();
    }

    // -----------------------------------------------------------------------
    // Owner FK (cascade delete)
    // -----------------------------------------------------------------------

    [Fact]
    public void Pet_HasForeignKeyToOwner()
    {
        var fk = _pet.GetForeignKeys()
                     .SingleOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Owner));

        fk.ShouldNotBeNull();
    }

    [Fact]
    public void Pet_OwnerForeignKey_IsCascadeDelete()
    {
        var fk = _pet.GetForeignKeys()
                     .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Owner));

        fk.DeleteBehavior.ShouldBe(DeleteBehavior.Cascade);
    }

    // -----------------------------------------------------------------------
    // Likes
    // -----------------------------------------------------------------------

    [Fact]
    public void Likes_IsRequired()
    {
        _pet.FindProperty(nameof(Pet.Likes))!
            .IsNullable
            .ShouldBeFalse();
    }

    [Fact]
    public void Likes_HasDefaultValueOfZero()
    {
        _pet.FindProperty(nameof(Pet.Likes))!
            .GetDefaultValue()
            .ShouldBe(0);
    }

}
