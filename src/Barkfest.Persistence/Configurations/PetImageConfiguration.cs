using Barkfest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barkfest.Persistence.Configurations;

public class PetImageConfiguration : IEntityTypeConfiguration<PetImage>
{
    public void Configure(EntityTypeBuilder<PetImage> builder)
    {
        builder.ToTable("PetImages");

        builder.HasKey(pi => pi.Id);
        builder.Property(pi => pi.Id)
            .HasColumnName("PetImageId")
            .HasDefaultValueSql("newsequentialid()");

        builder.Property(pi => pi.PetId)
            .HasColumnName("PetId");

        builder.Property(pi => pi.BlobName)
            .HasMaxLength(PetImage.BlobNameMaxLength)
            .IsRequired();

        builder.Property(pi => pi.ContentType)
            .HasMaxLength(PetImage.ContentTypeMaxLength)
            .IsRequired();

        builder.Property(pi => pi.DisplayOrder)
            .IsRequired();

        builder.Property(pi => pi.IsFeaturedImage)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(pi => pi.Pet)
            .WithMany(p => p.Images)
            .HasForeignKey(pi => pi.PetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
