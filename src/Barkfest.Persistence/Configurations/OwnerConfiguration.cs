using Barkfest.Domain.Entities;
using Barkfest.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barkfest.Persistence.Configurations;

public class OwnerConfiguration : IEntityTypeConfiguration<Owner>
{
    public void Configure(EntityTypeBuilder<Owner> builder)
    {
        builder.ToTable("Owners");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .HasColumnName("OwnerId")
            .HasDefaultValueSql("newsequentialid()");

        builder.Property(o => o.Username)
            .HasMaxLength(AccountConstraints.UsernameMaxLength)
            .IsRequired();

        builder.Property(o => o.DisplayName)
            .HasMaxLength(Owner.DisplayNameMaxLength);

        builder.Property(o => o.FirstName)
            .HasMaxLength(Owner.FirstNameMaxLength)
            .IsRequired();

        builder.Property(o => o.LastName)
            .HasMaxLength(Owner.LastNameMaxLength)
            .IsRequired();

        builder.Property(o => o.Email)
            .HasMaxLength(AccountConstraints.EmailMaxLength)
            .IsRequired();

        builder.Property(o => o.PhoneNumber)
            .HasMaxLength(E164PhoneNumber.MaxLength);

        builder.Property(o => o.PasswordHash)
            .IsRequired();

        builder.Property(o => o.IsEmailVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(o => o.VerificationToken)
            .IsRequired(false);

        builder.Property(o => o.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(o => o.IsVisible)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(o => o.Username)
            .IsUnique();

        builder.HasIndex(o => o.Email)
            .IsUnique();

        builder.OwnsOne(o => o.ProfileImage, pi =>
        {
            pi.Property(p => p.BlobName)
                .HasColumnName("ProfileImageBlobName")
                .HasMaxLength(500)
                .IsRequired(false);

            pi.Property(p => p.ContentType)
                .HasColumnName("ProfileImageContentType")
                .HasMaxLength(100)
                .IsRequired(false);
        });

    }
}
