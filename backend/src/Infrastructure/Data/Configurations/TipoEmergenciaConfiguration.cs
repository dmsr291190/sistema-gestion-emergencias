using Sige.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sige.Infrastructure.Data.Configurations;

public class TipoEmergenciaConfiguration : IEntityTypeConfiguration<TipoEmergencia>
{
    public void Configure(EntityTypeBuilder<TipoEmergencia> builder)
    {
        // data-model.md: TipoEmergencia.Nombre requerido, unico, max 100 caracteres.
        builder.Property(t => t.Nombre).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Icono).HasMaxLength(50);
        builder.Property(t => t.Color).HasMaxLength(20);
        builder.Property(t => t.Ambito).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.PrioridadPorDefecto).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(t => t.Nombre).IsUnique();
    }
}
