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

        builder.HasOne(u => u.Institucion)
            .WithMany()
            .HasForeignKey(u => u.InstitucionId)
            .OnDelete(DeleteBehavior.SetNull);

        // FR-104: opcional/aditivo para UnidadRespuesta (a diferencia de
        // Emergencia) -- ver data-model.md.
        builder.OwnsOne(u => u.Ubicacion, ub =>
        {
            ub.Property(x => x.Departamento).HasMaxLength(100);
            ub.Property(x => x.Provincia).HasMaxLength(100);
            ub.Property(x => x.Distrito).HasMaxLength(100);
            ub.Property(x => x.CentroPoblado).HasMaxLength(150);
            ub.Property(x => x.Direccion).HasMaxLength(250);
            ub.Property(x => x.Referencia).HasMaxLength(250);
            ub.Property(x => x.Ambito).HasConversion<string>().HasMaxLength(20);
        });
    }
}
