using Sige.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sige.Infrastructure.Data.Configurations;

public class AsignacionConfiguration : IEntityTypeConfiguration<Asignacion>
{
    public void Configure(EntityTypeBuilder<Asignacion> builder)
    {
        builder.Property(a => a.EstadoAsignacion).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(a => a.Unidad)
            .WithMany(u => u.Asignaciones)
            .HasForeignKey(a => a.UnidadId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
