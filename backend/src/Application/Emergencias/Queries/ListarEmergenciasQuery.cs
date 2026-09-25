using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.Emergencias.Queries;

// FR-003, FR-011, FR-108: emergencias para el mapa operativo y el dashboard, con
// filtros opcionales para el mapa avanzado (US4).
// Constitution Principio V: todo acceso MUST requerir autenticacion. Hallazgo de
// Converge: esta query no tenia [Authorize] y respondia sin token.
[Authorize]
public record ListarEmergenciasQuery : IRequest<List<EmergenciaDto>>
{
    public int? TipoEmergenciaId { get; init; }
    public Prioridad? Prioridad { get; init; }
    public EstadoEmergencia? Estado { get; init; }
    public Ambito? Ambito { get; init; }
    public string? Departamento { get; init; }
    public string? Provincia { get; init; }
}

public class ListarEmergenciasQueryHandler : IRequestHandler<ListarEmergenciasQuery, List<EmergenciaDto>>
{
    private readonly IApplicationDbContext _context;

    public ListarEmergenciasQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmergenciaDto>> Handle(ListarEmergenciasQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Emergencias.AsQueryable();

        if (request.TipoEmergenciaId.HasValue) query = query.Where(e => e.TipoEmergenciaId == request.TipoEmergenciaId);
        if (request.Prioridad.HasValue) query = query.Where(e => e.Prioridad == request.Prioridad);
        if (request.Estado.HasValue) query = query.Where(e => e.Estado == request.Estado);
        if (request.Ambito.HasValue) query = query.Where(e => e.Ubicacion.Ambito == request.Ambito);
        if (!string.IsNullOrWhiteSpace(request.Departamento)) query = query.Where(e => e.Ubicacion.Departamento == request.Departamento);
        if (!string.IsNullOrWhiteSpace(request.Provincia)) query = query.Where(e => e.Ubicacion.Provincia == request.Provincia);

        return await query
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
