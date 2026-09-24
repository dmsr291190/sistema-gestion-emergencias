namespace Sige.Application.Dashboard.Queries;

// FR-011: indicadores operativos basicos del dashboard.
public class DashboardIndicadoresDto
{
    public required Dictionary<string, int> EmergenciasPorEstado { get; init; }

    public required Dictionary<string, int> EmergenciasPorPrioridad { get; init; }

    public int UnidadesDisponibles { get; init; }

    public int UnidadesOcupadas { get; init; }

    public int UnidadesFueraDeServicio { get; init; }
}
