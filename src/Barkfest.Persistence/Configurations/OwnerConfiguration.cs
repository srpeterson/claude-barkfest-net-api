using Barkfest.Domain.Entities;
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

        builder.Property(o => o.FirstName)
            .HasMaxLength(Owner.FirstNameMaxLength)
            .IsRequired();

        builder.Property(o => o.LastName)
            .HasMaxLength(Owner.LastNameMaxLength)
            .IsRequired();

        builder.Property(o => o.Email)
            .HasMaxLength(Owner.EmailMaxLength)
            .IsRequired();

        builder.Property(o => o.PhoneNumber);

        builder.Property(o => o.PasswordHash)
            .IsRequired();

        builder.Property(o => o.Active)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(o => o.IsVisible)
            .IsRequired()
            .HasDefaultValue(true);

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
