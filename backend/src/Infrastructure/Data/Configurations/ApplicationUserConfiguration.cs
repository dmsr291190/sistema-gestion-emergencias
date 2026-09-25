using Sige.Domain.Entities;
using Sige.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sige.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.NombreCompleto).HasMaxLength(200).IsRequired();

        builder.HasOne(u => u.Institucion)
            .WithMany()
            .HasForeignKey(u => u.InstitucionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
