using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.Emergencias.Queries;

// US1 (detalle), FR-008 (timeline), US4.
// Constitution Principio V: todo acceso MUST requerir autenticacion. Hallazgo de
// Converge: esta query no tenia [Authorize] y respondia sin token.
[Authorize]
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
                TipoEmergencia = e.TipoEmergencia == null ? null : new TipoEmergenciaResumenDto
                {
                    Id = e.TipoEmergencia.Id,
                    Nombre = e.TipoEmergencia.Nombre,
                    Ambito = e.TipoEmergencia.Ambito,
                    Icono = e.TipoEmergencia.Icono,
                    Color = e.TipoEmergencia.Color
                },
                Descripcion = e.Descripcion,
                Ubicacion = new UbicacionDto
                {
                    Departamento = e.Ubicacion.Departamento,
                    Provincia = e.Ubicacion.Provincia,
                    Distrito = e.Ubicacion.Distrito,
                    CentroPoblado = e.Ubicacion.CentroPoblado,
                    Direccion = e.Ubicacion.Direccion,
                    Referencia = e.Ubicacion.Referencia,
                    Latitud = e.Ubicacion.Latitud,
                    Longitud = e.Ubicacion.Longitud,
                    Ambito = e.Ubicacion.Ambito,
                    SinDireccionFormal = e.Ubicacion.SinDireccionFormal
                },
                Afectados = e.Afectados,
                Heridos = e.Heridos,
                Desaparecidos = e.Desaparecidos,
                Fallecidos = e.Fallecidos,
                Evacuados = e.Evacuados,
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
