using Sige.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.Emergencias.Queries;

// US1 (detalle), FR-008 (timeline), US4.
public record ObtenerEmergenciaPorIdQuery : IRequest<EmergenciaDetailDto?>
{
    public required int EmergenciaId { get; init; }
}

public class ObtenerEmergenciaPorIdQueryHandler : IRequestHandler<ObtenerEmergenciaPorIdQuery, EmergenciaDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public ObtenerEmergenciaPorIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmergenciaDetailDto?> Handle(ObtenerEmergenciaPorIdQuery request, CancellationToken cancellationToken)
    {
        var emergencia = await _context.Emergencias
            .Where(e => e.Id == request.EmergenciaId)
            .Select(e => new EmergenciaDetailDto
            {
                Id = e.Id,
                Tipo = e.Tipo,
                Descripcion = e.Descripcion,
                Latitud = e.Latitud,
                Longitud = e.Longitud,
                Prioridad = e.Prioridad,
                Estado = e.Estado,
                FechaHoraReporte = e.FechaHoraReporte,
                ReportanteNombre = e.ReportanteNombre,
                ReportanteContacto = e.ReportanteContacto,
                Asignaciones = e.Asignaciones.Select(a => new AsignacionDto
                {
                    Id = a.Id,
                    UnidadId = a.UnidadId,
                    UnidadIdentificador = a.Unidad!.Identificador,
                    UnidadTipo = a.Unidad!.Tipo,
                    EstadoAsignacion = a.EstadoAsignacion,
                    FechaHoraAsignacion = a.FechaHoraAsignacion
                }).ToList(),
                Timeline = e.EventosAuditoria
                    .OrderBy(ev => ev.FechaHora)
                    .Select(ev => new EventoAuditoriaDto
                    {
                        Id = ev.Id,
                        TipoEvento = ev.TipoEvento,
                        EstadoAnterior = ev.EstadoAnterior,
                        EstadoNuevo = ev.EstadoNuevo,
                        UsuarioId = ev.UsuarioId,
                        FechaHora = ev.FechaHora
                    }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return emergencia;
    }
}
