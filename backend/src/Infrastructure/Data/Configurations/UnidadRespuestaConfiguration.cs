using Sige.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sige.Infrastructure.Data.Configurations;

public class UnidadRespuestaConfiguration : IEntityTypeConfiguration<UnidadRespuesta>
{
    public void Configure(EntityTypeBuilder<UnidadRespuesta> builder)
    {
        builder.Property(u => u.Identificador).HasMaxLength(50).IsRequired();
        builder.Property(u => u.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(u => u.EstadoOperativo).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(u => u.Identificador).IsUnique();
    }
}
