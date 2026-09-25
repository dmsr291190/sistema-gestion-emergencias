using Sige.Domain.Entities;

namespace Sige.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Emergencia> Emergencias { get; }

    DbSet<UnidadRespuesta> UnidadesRespuesta { get; }

    DbSet<Asignacion> Asignaciones { get; }

    DbSet<EventoAuditoria> EventosAuditoria { get; }

    DbSet<TipoEmergencia> TiposEmergencia { get; }

    DbSet<Institucion> Instituciones { get; }

    DbSet<Personal> Personal { get; }

    DbSet<Recurso> Recursos { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
