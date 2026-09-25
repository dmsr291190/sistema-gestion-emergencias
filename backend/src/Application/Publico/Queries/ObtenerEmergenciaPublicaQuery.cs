using Sige.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.Publico.Queries;

// FR-113: sin [Authorize]. "codigo" es el Id interno de la emergencia -- esta
// ampliacion no introduce un identificador publico separado (no modelado en
// data-model.md); el Edge Case de "no revelar por enumeracion" se cumple
// devolviendo null (-> 404 generico en el endpoint) tanto si el id no existe
// como si en el futuro se agrega un criterio de exclusion del alcance publico.
public record ObtenerEmergenciaPublicaQuery : IRequest<EmergenciaPublicaDetalleDto?>
{
    public required int Codigo { get; init; }
}

public class ObtenerEmergenciaPublicaQueryHandler : IRequestHandler<ObtenerEmergenciaPublicaQuery, EmergenciaPublicaDetalleDto?>
{
    private readonly IApplicationDbContext _context;

    public ObtenerEmergenciaPublicaQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmergenciaPublicaDetalleDto?> Handle(ObtenerEmergenciaPublicaQuery request, CancellationToken cancellationToken)
    {
        var crudo = await _context.Emergencias
            .Where(e => e.Id == request.Codigo)
            .Select(e => new
            {
                e.Id,
                TipoNombre = e.TipoEmergencia == null ? null : e.TipoEmergencia.Nombre,
                TipoIcono = e.TipoEmergencia == null ? null : e.TipoEmergencia.Icono,
                TipoColor = e.TipoEmergencia == null ? null : e.TipoEmergencia.Color,
                e.Prioridad,
                e.Estado,
                e.Ubicacion.Departamento,
                e.Ubicacion.Provincia,
                e.Ubicacion.Distrito,
                e.Ubicacion.CentroPoblado,
                e.Ubicacion.Latitud,
                e.Ubicacion.Longitud,
                e.Ubicacion.Ambito,
                e.LastModified,
                // FR-114: solo TipoEvento/EstadoNuevo/FechaHora -- nunca UsuarioId
                // (dato interno) ni EstadoAnterior (detalle operativo interno).
                Timeline = e.EventosAuditoria
                    .OrderBy(ev => ev.FechaHora)
                    .Select(ev => new EventoPublicoDto
                    {
                        TipoEvento = ev.TipoEvento,
                        EstadoNuevo = ev.EstadoNuevo,
                        FechaHora = ev.FechaHora
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (crudo == null)
        {
            return null;
        }

        return new EmergenciaPublicaDetalleDto
        {
            Codigo = crudo.Id,
            TipoNombre = crudo.TipoNombre,
            TipoIcono = crudo.TipoIcono,
            TipoColor = crudo.TipoColor,
            Prioridad = crudo.Prioridad,
            Estado = crudo.Estado,
            Ubicacion = new UbicacionPublicaDto
            {
                Departamento = crudo.Departamento,
                Provincia = crudo.Provincia,
                Distrito = crudo.Distrito,
                CentroPoblado = crudo.CentroPoblado,
                LatitudAproximada = Math.Round(crudo.Latitud, 1),
                LongitudAproximada = Math.Round(crudo.Longitud, 1),
                Ambito = crudo.Ambito
            },
            UltimaActualizacion = crudo.LastModified,
            Timeline = crudo.Timeline
        };
    }
}
