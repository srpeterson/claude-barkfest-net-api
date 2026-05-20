using Barkfest.Domain.Entities;
using Barkfest.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barkfest.Persistence.Configurations;

public class AdministratorConfiguration : IEntityTypeConfiguration<Administrator>
{
    public void Configure(EntityTypeBuilder<Administrator> builder)
    {
        builder.ToTable("Administrators");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("AdministratorId")
            .HasDefaultValueSql("newsequentialid()");

        builder.Property(a => a.Username)
            .HasMaxLength(AccountConstraints.UsernameMaxLength)
            .IsRequired();

        builder.Property(a => a.Name)
            .HasMaxLength(Administrator.NameMaxLength)
            .IsRequired();

        builder.Property(a => a.Email)
            .HasMaxLength(AccountConstraints.EmailMaxLength)
            .IsRequired();

        builder.Property(a => a.PhoneNumber)
            .HasMaxLength(E164PhoneNumber.MaxLength)
            .IsRequired();

        builder.Property(a => a.PasswordHash)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.HasIndex(a => a.Username)
            .IsUnique();

        builder.HasIndex(a => a.Email)
            .IsUnique();
    }
}
