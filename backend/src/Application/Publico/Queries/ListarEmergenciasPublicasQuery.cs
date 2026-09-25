using Sige.Application.Common.Interfaces;
using Sige.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.Publico.Queries;

// FR-113: sin [Authorize] -- accesible sin autenticacion. Solo emergencias
// "activas" (no Cerradas). El redondeo de coordenadas (FR-115) se hace en
// memoria, no en la consulta SQL, para no depender de que el proveedor de base
// de datos traduzca Math.Round a SQL.
public record ListarEmergenciasPublicasQuery : IRequest<List<EmergenciaPublicaDto>>;

public class ListarEmergenciasPublicasQueryHandler : IRequestHandler<ListarEmergenciasPublicasQuery, List<EmergenciaPublicaDto>>
{
    private readonly IApplicationDbContext _context;

    public ListarEmergenciasPublicasQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmergenciaPublicaDto>> Handle(ListarEmergenciasPublicasQuery request, CancellationToken cancellationToken)
    {
        var crudo = await _context.Emergencias
            .Where(e => e.Estado != EstadoEmergencia.Cerrada)
            .OrderByDescending(e => e.LastModified)
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
                e.LastModified
            })
            .ToListAsync(cancellationToken);

        return crudo.Select(e => new EmergenciaPublicaDto
        {
            Codigo = e.Id,
            TipoNombre = e.TipoNombre,
            TipoIcono = e.TipoIcono,
            TipoColor = e.TipoColor,
            Prioridad = e.Prioridad,
            Estado = e.Estado,
            Ubicacion = new UbicacionPublicaDto
            {
                Departamento = e.Departamento,
                Provincia = e.Provincia,
                Distrito = e.Distrito,
                CentroPoblado = e.CentroPoblado,
                LatitudAproximada = Math.Round(e.Latitud, 1),
                LongitudAproximada = Math.Round(e.Longitud, 1),
                Ambito = e.Ambito
            },
            UltimaActualizacion = e.LastModified
        }).ToList();
    }
}
