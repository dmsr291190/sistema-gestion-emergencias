using Sige.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sige.Infrastructure.Data.Configurations;

public class PersonalConfiguration : IEntityTypeConfiguration<Personal>
{
    public void Configure(EntityTypeBuilder<Personal> builder)
    {
        builder.Property(p => p.Nombres).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Apellidos).HasMaxLength(150).IsRequired();
        // data-model.md: Personal.Documento requerido.
        builder.Property(p => p.Documento).HasMaxLength(20).IsRequired();
        builder.Property(p => p.Especialidad).HasMaxLength(100);
        builder.Property(p => p.Funcion).HasMaxLength(100);
        builder.Property(p => p.Certificaciones).HasMaxLength(500);

        builder.HasOne(p => p.Institucion)
            .WithMany()
            .HasForeignKey(p => p.InstitucionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.UnidadRespuesta)
            .WithMany(u => u.Personal)
            .HasForeignKey(p => p.UnidadRespuestaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
