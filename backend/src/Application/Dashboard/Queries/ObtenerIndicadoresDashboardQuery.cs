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
            .Select(e => new { e.Estado, e.Prioridad, e.Ubicacion.Ambito })
            .ToListAsync(cancellationToken);

        var emergenciasPorEstado = emergenciasActivas
            .GroupBy(e => e.Estado)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var emergenciasPorPrioridad = emergenciasActivas
            .GroupBy(e => e.Prioridad)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        // FR-127: desglose por ambito (terrestre/maritimo/aereo/mixto).
        var emergenciasPorAmbito = emergenciasActivas
            .GroupBy(e => e.Ambito)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var unidades = await _context.UnidadesRespuesta
            .Select(u => u.EstadoOperativo)
            .ToListAsync(cancellationToken);

        // FR-127: tiempo promedio de atencion, entre las emergencias Cerradas, desde
        // FechaHoraReporte hasta el evento "EmergenciaCerrada" mas reciente de cada una.
        var cierres = await _context.Emergencias
            .Where(e => e.Estado == EstadoEmergencia.Cerrada)
            .Select(e => new
            {
                e.FechaHoraReporte,
                FechaCierre = e.EventosAuditoria
                    .Where(ev => ev.TipoEvento == "EmergenciaCerrada")
                    .Max(ev => (DateTimeOffset?)ev.FechaHora)
            })
            .Where(e => e.FechaCierre != null)
            .ToListAsync(cancellationToken);

        double? tiempoPromedioAtencionMinutos = cierres.Count > 0
            ? cierres.Average(c => (c.FechaCierre!.Value - c.FechaHoraReporte).TotalMinutes)
            : null;

        // FR-127: personal cuya unidad esta actualmente desplegada (Ocupada).
        var personalDesplegado = await _context.Personal
            .CountAsync(p => p.UnidadRespuesta.EstadoOperativo == EstadoOperativoUnidad.Ocupada, cancellationToken);

        // FR-127: lo que esta actualmente en uso (Cantidad - CantidadDisponible), no en base.
        var recursos = await _context.Recursos
            .Select(r => new { r.Cantidad, r.CantidadDisponible })
            .ToListAsync(cancellationToken);
        var recursosMovilizados = recursos.Sum(r => r.Cantidad - r.CantidadDisponible);

        return new DashboardIndicadoresDto
        {
            EmergenciasPorEstado = emergenciasPorEstado,
            EmergenciasPorPrioridad = emergenciasPorPrioridad,
            UnidadesDisponibles = unidades.Count(u => u == EstadoOperativoUnidad.Disponible),
            UnidadesOcupadas = unidades.Count(u => u == EstadoOperativoUnidad.Ocupada),
            UnidadesFueraDeServicio = unidades.Count(u => u == EstadoOperativoUnidad.FueraDeServicio),
            EmergenciasPorAmbito = emergenciasPorAmbito,
            TiempoPromedioAtencionMinutos = tiempoPromedioAtencionMinutos,
            PersonalDesplegado = personalDesplegado,
            RecursosMovilizados = recursosMovilizados
        };
    }
}
