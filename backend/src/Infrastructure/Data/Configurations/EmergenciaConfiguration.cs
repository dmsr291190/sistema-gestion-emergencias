using Sige.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sige.Infrastructure.Data.Configurations;

public class EmergenciaConfiguration : IEntityTypeConfiguration<Emergencia>
{
    public void Configure(EntityTypeBuilder<Emergencia> builder)
    {
        // Columna legada (FR-103): ahora nullable, ya no es la fuente de verdad.
        builder.Property(e => e.Tipo).HasMaxLength(100);
        builder.Property(e => e.Descripcion).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.ReportanteNombre).HasMaxLength(200).IsRequired();
        builder.Property(e => e.ReportanteContacto).HasMaxLength(100);
        builder.Property(e => e.Estado).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Prioridad).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(e => e.TipoEmergencia)
            .WithMany()
            .HasForeignKey(e => e.TipoEmergenciaId)
            .OnDelete(DeleteBehavior.Restrict);

        // FR-104: value object embebido, reemplaza los campos sueltos lat/lon del MVP.
        builder.OwnsOne(e => e.Ubicacion, ub =>
        {
            ub.Property(u => u.Departamento).HasMaxLength(100);
            ub.Property(u => u.Provincia).HasMaxLength(100);
            ub.Property(u => u.Distrito).HasMaxLength(100);
            ub.Property(u => u.CentroPoblado).HasMaxLength(150);
            ub.Property(u => u.Direccion).HasMaxLength(250);
            ub.Property(u => u.Referencia).HasMaxLength(250);
            ub.Property(u => u.Ambito).HasConversion<string>().HasMaxLength(20);
        });

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
