using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.Dashboard.Queries;

// Constitution Principio V: todo acceso MUST requerir autenticacion. Hallazgo de
// Converge: esta query no tenia [Authorize] y respondia sin token.
[Authorize]
public record ObtenerIndicadoresDashboardQuery : IRequest<DashboardIndicadoresDto>;

public class ObtenerIndicadoresDashboardQueryHandler
    : IRequestHandler<ObtenerIndicadoresDashboardQuery, DashboardIndicadoresDto>
{
    private readonly IApplicationDbContext _context;

    public ObtenerIndicadoresDashboardQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardIndicadoresDto> Handle(ObtenerIndicadoresDashboardQuery request, CancellationToken cancellationToken)
    {
        // FR-011: solo emergencias activas (no "Cerrada") cuentan para los indicadores.
        var emergenciasActivas = await _context.Emergencias
            .Where(e => e.Estado != EstadoEmergencia.Cerrada)
            .Select(e => new { e.Estado, e.Prioridad })
            .ToListAsync(cancellationToken);

        var emergenciasPorEstado = emergenciasActivas
            .GroupBy(e => e.Estado)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var emergenciasPorPrioridad = emergenciasActivas
            .GroupBy(e => e.Prioridad)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var unidades = await _context.UnidadesRespuesta
            .Select(u => u.EstadoOperativo)
            .ToListAsync(cancellationToken);

        return new DashboardIndicadoresDto
        {
            EmergenciasPorEstado = emergenciasPorEstado,
            EmergenciasPorPrioridad = emergenciasPorPrioridad,
            UnidadesDisponibles = unidades.Count(u => u == EstadoOperativoUnidad.Disponible),
            UnidadesOcupadas = unidades.Count(u => u == EstadoOperativoUnidad.Ocupada),
            UnidadesFueraDeServicio = unidades.Count(u => u == EstadoOperativoUnidad.FueraDeServicio)
        };
    }
}
