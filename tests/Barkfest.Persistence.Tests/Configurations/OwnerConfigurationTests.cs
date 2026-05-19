using Barkfest.Domain.Entities;
using Barkfest.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Barkfest.Persistence.Tests.Configurations;

public class OwnerConfigurationTests
{
    private readonly IEntityType _owner =
        ModelHelper.Model.FindEntityType(typeof(Owner))!;

    private StoreObjectIdentifier TableId =>
        StoreObjectIdentifier.Table(_owner.GetTableName()!, _owner.GetSchema());

    // -----------------------------------------------------------------------
    // Table
    // -----------------------------------------------------------------------

    [Fact]
    public void Owner_MapsToOwnersTable()
    {
        _owner.GetTableName().ShouldBe("Owners");
    }

    // -----------------------------------------------------------------------
    // Primary key
    // -----------------------------------------------------------------------

    [Fact]
    public void Id_MapsToOwnerIdColumn()
    {
        _owner.FindProperty(nameof(Owner.Id))!
              .GetColumnName(TableId)
              .ShouldBe("OwnerId");
    }

    [Fact]
    public void Id_HasDatabaseGeneratedDefault()
    {
        var prop = _owner.FindProperty(nameof(Owner.Id))!;
        prop.GetDefaultValueSql().ShouldBe("newsequentialid()");
    }

    // -----------------------------------------------------------------------
    // Username
    // -----------------------------------------------------------------------

    [Fact]
    public void Username_HasCorrectMaxLength()
    {
        _owner.FindProperty(nameof(Owner.Username))!
              .GetMaxLength()
              .ShouldBe(AccountConstraints.UsernameMaxLength);
    }

    [Fact]
    public void Username_IsRequired()
    {
        _owner.FindProperty(nameof(Owner.Username))!
              .IsNullable
              .ShouldBeFalse();
    }

    [Fact]
    public void Username_HasUniqueIndex()
    {
        var index = _owner.GetIndexes()
            .SingleOrDefault(i =>
                i.Properties.Count == 1 &&
                i.Properties[0].Name == nameof(Owner.Username));

        index.ShouldNotBeNull();
        index!.IsUnique.ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // FirstName
    // -----------------------------------------------------------------------

    [Fact]
    public void FirstName_HasCorrectMaxLength()
    {
        _owner.FindProperty(nameof(Owner.FirstName))!
              .GetMaxLength()
              .ShouldBe(Owner.FirstNameMaxLength);
    }

    [Fact]
    public void FirstName_IsRequired()
    {
        _owner.FindProperty(nameof(Owner.FirstName))!
              .IsNullable
              .ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // LastName
    // -----------------------------------------------------------------------

    [Fact]
    public void LastName_HasCorrectMaxLength()
    {
        _owner.FindProperty(nameof(Owner.LastName))!
              .GetMaxLength()
              .ShouldBe(Owner.LastNameMaxLength);
    }

    [Fact]
    public void LastName_IsRequired()
    {
        _owner.FindProperty(nameof(Owner.LastName))!
              .IsNullable
              .ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // Email
    // -----------------------------------------------------------------------

    [Fact]
    public void Email_HasCorrectMaxLength()
    {
        _owner.FindProperty(nameof(Owner.Email))!
              .GetMaxLength()
              .ShouldBe(AccountConstraints.EmailMaxLength);
    }

    [Fact]
    public void Email_IsRequired()
    {
        _owner.FindProperty(nameof(Owner.Email))!
              .IsNullable
              .ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // PhoneNumber
    // -----------------------------------------------------------------------

    [Fact]
    public void PhoneNumber_IsNullable()
    {
        _owner.FindProperty(nameof(Owner.PhoneNumber))!
              .IsNullable
              .ShouldBeTrue();
    }

    [Fact]
    public void PhoneNumber_HasCorrectMaxLength()
    {
        _owner.FindProperty(nameof(Owner.PhoneNumber))!
              .GetMaxLength()
              .ShouldBe(E164PhoneNumber.MaxLength);
    }

    // -----------------------------------------------------------------------
    // ProfileImage (owned entity — shares Owners table)
    // -----------------------------------------------------------------------

    [Fact]
    public void ProfileImage_BlobName_MapsToCorrectColumn()
    {
        var owned = _owner.FindNavigation(nameof(Owner.ProfileImage))!.TargetEntityType;
        owned.FindProperty(nameof(ProfileImage.BlobName))!
             .GetColumnName(TableId)
             .ShouldBe("ProfileImageBlobName");
    }

    [Fact]
    public void ProfileImage_BlobName_HasMaxLength500()
    {
        var owned = _owner.FindNavigation(nameof(Owner.ProfileImage))!.TargetEntityType;
        owned.FindProperty(nameof(ProfileImage.BlobName))!
             .GetMaxLength()
             .ShouldBe(500);
    }

    [Fact]
    public void ProfileImage_BlobName_IsNullable()
    {
        var owned = _owner.FindNavigation(nameof(Owner.ProfileImage))!.TargetEntityType;
        owned.FindProperty(nameof(ProfileImage.BlobName))!
             .IsNullable
             .ShouldBeTrue();
    }

    [Fact]
    public void ProfileImage_ContentType_MapsToCorrectColumn()
    {
        var owned = _owner.FindNavigation(nameof(Owner.ProfileImage))!.TargetEntityType;
        owned.FindProperty(nameof(ProfileImage.ContentType))!
             .GetColumnName(TableId)
             .ShouldBe("ProfileImageContentType");
    }

    [Fact]
    public void ProfileImage_ContentType_HasMaxLength100()
    {
        var owned = _owner.FindNavigation(nameof(Owner.ProfileImage))!.TargetEntityType;
        owned.FindProperty(nameof(ProfileImage.ContentType))!
             .GetMaxLength()
             .ShouldBe(100);
    }

    [Fact]
    public void ProfileImage_ContentType_IsNullable()
    {
        var owned = _owner.FindNavigation(nameof(Owner.ProfileImage))!.TargetEntityType;
        owned.FindProperty(nameof(ProfileImage.ContentType))!
             .IsNullable
             .ShouldBeTrue();
    }
}
