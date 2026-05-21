using Barkfest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Barkfest.Persistence.Tests.Configurations;

public class PetImageConfigurationTests
{
    private readonly IEntityType _petImage =
        ModelHelper.Model.FindEntityType(typeof(PetImage))!;

    private StoreObjectIdentifier TableId =>
        StoreObjectIdentifier.Table(_petImage.GetTableName()!, _petImage.GetSchema());

    // -----------------------------------------------------------------------
    // Table
    // -----------------------------------------------------------------------

    [Fact]
    public void PetImage_MapsToPetImagesTable()
    {
        _petImage.GetTableName().ShouldBe("PetImages");
    }

    // -----------------------------------------------------------------------
    // Primary key
    // -----------------------------------------------------------------------

    [Fact]
    public void Id_MapsToPetImageIdColumn()
    {
        _petImage.FindProperty(nameof(PetImage.Id))!
                 .GetColumnName(TableId)
                 .ShouldBe("PetImageId");
    }

    [Fact]
    public void Id_HasDatabaseGeneratedDefault()
    {
        _petImage.FindProperty(nameof(PetImage.Id))!
                 .GetDefaultValueSql()
                 .ShouldBe("newsequentialid()");
    }

    // -----------------------------------------------------------------------
    // BlobName
    // -----------------------------------------------------------------------

    [Fact]
    public void BlobName_HasCorrectMaxLength()
    {
        _petImage.FindProperty(nameof(PetImage.BlobName))!
                 .GetMaxLength()
                 .ShouldBe(PetImage.BlobNameMaxLength);
    }

    [Fact]
    public void BlobName_IsRequired()
    {
        _petImage.FindProperty(nameof(PetImage.BlobName))!
                 .IsNullable
                 .ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // ContentType
    // -----------------------------------------------------------------------

    [Fact]
    public void ContentType_HasCorrectMaxLength()
    {
        _petImage.FindProperty(nameof(PetImage.ContentType))!
                 .GetMaxLength()
                 .ShouldBe(PetImage.ContentTypeMaxLength);
    }

    [Fact]
    public void ContentType_IsRequired()
    {
        _petImage.FindProperty(nameof(PetImage.ContentType))!
                 .IsNullable
                 .ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // DisplayOrder
    // -----------------------------------------------------------------------

    [Fact]
    public void DisplayOrder_IsRequired()
    {
        _petImage.FindProperty(nameof(PetImage.DisplayOrder))!
                 .IsNullable
                 .ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // IsFeaturedImage
    // -----------------------------------------------------------------------

    [Fact]
    public void IsFeaturedImage_IsRequired()
    {
        _petImage.FindProperty(nameof(PetImage.IsFeaturedImage))!
                 .IsNullable
                 .ShouldBeFalse();
    }

    [Fact]
    public void IsFeaturedImage_DefaultsToFalse()
    {
        _petImage.FindProperty(nameof(PetImage.IsFeaturedImage))!
                 .GetDefaultValue()
                 .ShouldBe(false);
    }

    // -----------------------------------------------------------------------
    // Pet FK (cascade delete)
    // -----------------------------------------------------------------------

    [Fact]
    public void PetImage_HasForeignKeyToPet()
    {
        var fk = _petImage.GetForeignKeys()
                          .SingleOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Pet));

        fk.ShouldNotBeNull();
    }

    [Fact]
    public void PetImage_PetForeignKey_IsCascadeDelete()
    {
        var fk = _petImage.GetForeignKeys()
                          .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Pet));

        fk.DeleteBehavior.ShouldBe(DeleteBehavior.Cascade);
    }

    [Fact]
    public void PetImage_PetForeignKey_UsePetIdColumn()
    {
        var fk = _petImage.GetForeignKeys()
                          .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Pet));

        fk.Properties.ShouldContain(p => p.GetColumnName(TableId) == "PetId");
    }
}
