using Barkfest.Domain.Entities;
using Barkfest.Domain.ValueObjects;
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
    // Username
    // -----------------------------------------------------------------------

    [Fact]
    public void Username_HasCorrectMaxLength()
    {
        _administrator.FindProperty(nameof(Administrator.Username))!
                      .GetMaxLength()
                      .ShouldBe(AccountConstraints.UsernameMaxLength);
    }

    [Fact]
    public void Username_IsRequired()
    {
        _administrator.FindProperty(nameof(Administrator.Username))!
                      .IsNullable
                      .ShouldBeFalse();
    }

    [Fact]
    public void Username_HasUniqueIndex()
    {
        var index = _administrator.GetIndexes()
            .SingleOrDefault(i =>
                i.Properties.Count == 1 &&
                i.Properties[0].Name == nameof(Administrator.Username));

        index.ShouldNotBeNull();
        index!.IsUnique.ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // Name
    // -----------------------------------------------------------------------

    [Fact]
    public void Name_HasCorrectMaxLength()
    {
        _administrator.FindProperty(nameof(Administrator.Name))!
                      .GetMaxLength()
                      .ShouldBe(Administrator.NameMaxLength);
    }

    [Fact]
    public void Name_IsRequired()
    {
        _administrator.FindProperty(nameof(Administrator.Name))!
                      .IsNullable
                      .ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // Email
    // -----------------------------------------------------------------------

    [Fact]
    public void Email_HasCorrectMaxLength()
    {
        _administrator.FindProperty(nameof(Administrator.Email))!
                      .GetMaxLength()
                      .ShouldBe(AccountConstraints.EmailMaxLength);
    }

    [Fact]
    public void Email_IsRequired()
    {
        _administrator.FindProperty(nameof(Administrator.Email))!
                      .IsNullable
                      .ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // PhoneNumber
    // -----------------------------------------------------------------------

    [Fact]
    public void PhoneNumber_HasCorrectMaxLength()
    {
        _administrator.FindProperty(nameof(Administrator.PhoneNumber))!
                      .GetMaxLength()
                      .ShouldBe(E164PhoneNumber.MaxLength);
    }

    [Fact]
    public void PhoneNumber_IsRequired()
    {
        _administrator.FindProperty(nameof(Administrator.PhoneNumber))!
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
