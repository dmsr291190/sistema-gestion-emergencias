using Sige.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sige.Infrastructure.Data.Configurations;

public class EventoAuditoriaConfiguration : IEntityTypeConfiguration<EventoAuditoria>
{
    public void Configure(EntityTypeBuilder<EventoAuditoria> builder)
    {
        builder.Property(e => e.TipoEvento).HasMaxLength(50).IsRequired();
        builder.Property(e => e.EstadoAnterior).HasMaxLength(50);
        builder.Property(e => e.EstadoNuevo).HasMaxLength(50);

        builder.HasOne(e => e.Unidad)
            .WithMany()
            .HasForeignKey(e => e.UnidadId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
