using Sige.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sige.Infrastructure.Data.Configurations;

public class EmergenciaConfiguration : IEntityTypeConfiguration<Emergencia>
{
    public void Configure(EntityTypeBuilder<Emergencia> builder)
    {
        builder.Property(e => e.Tipo).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Descripcion).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.ReportanteNombre).HasMaxLength(200).IsRequired();
        builder.Property(e => e.ReportanteContacto).HasMaxLength(100);
        builder.Property(e => e.Estado).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Prioridad).HasConversion<string>().HasMaxLength(20);

        builder.HasMany(e => e.Asignaciones)
            .WithOne(a => a.Emergencia)
            .HasForeignKey(a => a.EmergenciaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.EventosAuditoria)
            .WithOne(ev => ev.Emergencia)
            .HasForeignKey(ev => ev.EmergenciaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
