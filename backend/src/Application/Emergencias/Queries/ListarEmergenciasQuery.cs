using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.Emergencias.Queries;

// FR-003, FR-011: emergencias para el mapa operativo y el dashboard.
// Constitution Principio V: todo acceso MUST requerir autenticacion. Hallazgo de
// Converge: esta query no tenia [Authorize] y respondia sin token.
[Authorize]
public record ListarEmergenciasQuery : IRequest<List<EmergenciaDto>>;

public class ListarEmergenciasQueryHandler : IRequestHandler<ListarEmergenciasQuery, List<EmergenciaDto>>
{
    private readonly IApplicationDbContext _context;

    public ListarEmergenciasQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmergenciaDto>> Handle(ListarEmergenciasQuery request, CancellationToken cancellationToken)
    {
        return await _context.Emergencias
            .OrderByDescending(e => e.FechaHoraReporte)
            .Select(e => new EmergenciaDto
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
                ReportanteContacto = e.ReportanteContacto
            })
            .ToListAsync(cancellationToken);
    }
}
