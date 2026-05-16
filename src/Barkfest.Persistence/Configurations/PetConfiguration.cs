using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barkfest.Persistence.Configurations;

public class PetConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.ToTable("Pets");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("PetId")
            .HasDefaultValueSql("newsequentialid()");

        builder.Property(p => p.Name)
            .HasMaxLength(Pet.NameMaxLength)
            .IsRequired();

        builder.Property(p => p.Description);

        builder.Property(p => p.DateOfBirth)
            .HasColumnType("date");

        builder.Property(p => p.PetType)
            .HasConversion(
                pt => pt.Value,
                v => PetType.FromValue(v))
            .IsRequired();

        builder.Ignore(p => p.Age);

        builder.OwnsOne(p => p.ProfileImage, pi =>
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

        builder.HasOne(p => p.Owner)
            .WithMany(o => o.Pets)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
