using Sige.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sige.Infrastructure.Data.Configurations;

public class RecursoConfiguration : IEntityTypeConfiguration<Recurso>
{
    public void Configure(EntityTypeBuilder<Recurso> builder)
    {
        builder.Property(r => r.Codigo).HasMaxLength(50).IsRequired();
        builder.Property(r => r.Nombre).HasMaxLength(150).IsRequired();
        builder.Property(r => r.Categoria).HasConversion<string>().HasMaxLength(30);
        builder.Property(r => r.UnidadMedida).HasMaxLength(30);

        builder.HasOne(r => r.UnidadRespuesta)
            .WithMany(u => u.Recursos)
            .HasForeignKey(r => r.UnidadRespuestaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
