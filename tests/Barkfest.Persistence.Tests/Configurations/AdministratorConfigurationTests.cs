using Barkfest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Barkfest.Persistence.Tests.Configurations;

public class AdministratorConfigurationTests
{
    private readonly IEntityType _administrator =
        ModelHelper.Model.FindEntityType(typeof(Administrator))!;

    private StoreObjectIdentifier TableId =>
        StoreObjectIdentifier.Table(_administrator.GetTableName()!, _administrator.GetSchema());

    // -----------------------------------------------------------------------
    // Table
    // -----------------------------------------------------------------------

    [Fact]
    public void Administrator_MapsToAdministratorsTable()
    {
        _administrator.GetTableName().ShouldBe("Administrators");
    }

    // -----------------------------------------------------------------------
    // Primary key
    // -----------------------------------------------------------------------

    [Fact]
    public void Id_MapsToAdministratorIdColumn()
    {
        _administrator.FindProperty(nameof(Administrator.Id))!
                      .GetColumnName(TableId)
                      .ShouldBe("AdministratorId");
    }

    [Fact]
    public void Id_HasDatabaseGeneratedDefault()
    {
        _administrator.FindProperty(nameof(Administrator.Id))!
                      .GetDefaultValueSql()
                      .ShouldBe("newsequentialid()");
    }

    // -----------------------------------------------------------------------
    // Email
    // -----------------------------------------------------------------------

    [Fact]
    public void Email_HasCorrectMaxLength()
    {
        _administrator.FindProperty(nameof(Administrator.Email))!
                      .GetMaxLength()
                      .ShouldBe(Administrator.EmailMaxLength);
    }

    [Fact]
    public void Email_IsRequired()
    {
        _administrator.FindProperty(nameof(Administrator.Email))!
                      .IsNullable
                      .ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // PasswordHash
    // -----------------------------------------------------------------------

    [Fact]
    public void PasswordHash_IsRequired()
    {
        _administrator.FindProperty(nameof(Administrator.PasswordHash))!
                      .IsNullable
                      .ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // CreatedAt
    // -----------------------------------------------------------------------

    [Fact]
    public void CreatedAt_IsRequired()
    {
        _administrator.FindProperty(nameof(Administrator.CreatedAt))!
                      .IsNullable
                      .ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // Unique index on Email
    // -----------------------------------------------------------------------

    [Fact]
    public void Email_HasUniqueIndex()
    {
        var index = _administrator.GetIndexes()
            .SingleOrDefault(i =>
                i.Properties.Count == 1 &&
                i.Properties[0].Name == nameof(Administrator.Email));

        index.ShouldNotBeNull();
        index!.IsUnique.ShouldBeTrue();
    }
}
