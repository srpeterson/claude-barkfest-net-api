using Barkfest.Domain.Entities;
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

        builder.Property(a => a.Email)
            .HasMaxLength(Administrator.EmailMaxLength)
            .IsRequired();

        builder.Property(a => a.PasswordHash)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.HasIndex(a => a.Email)
            .IsUnique();
    }
}
