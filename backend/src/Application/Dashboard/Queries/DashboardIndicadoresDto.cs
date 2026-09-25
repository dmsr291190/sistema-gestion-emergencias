namespace Sige.Application.Dashboard.Queries;

// FR-011: indicadores operativos basicos del dashboard (MVP).
// FR-127 (ampliacion 002, US8): desglose por ambito, tiempo promedio de atencion,
// personal desplegado y recursos movilizados.
public class DashboardIndicadoresDto
{
    public required Dictionary<string, int> EmergenciasPorEstado { get; init; }

    public required Dictionary<string, int> EmergenciasPorPrioridad { get; init; }

    public int UnidadesDisponibles { get; init; }

    public int UnidadesOcupadas { get; init; }

    public int UnidadesFueraDeServicio { get; init; }

    // FR-127
    public required Dictionary<string, int> EmergenciasPorAmbito { get; init; }

    // Promedio, entre las emergencias Cerradas, de (fecha del evento
    // "EmergenciaCerrada" - FechaHoraReporte). Null si no hay ninguna cerrada.
    public double? TiempoPromedioAtencionMinutos { get; init; }

    // Personal cuya unidad esta actualmente Ocupada (desplegada en una emergencia).
    public int PersonalDesplegado { get; init; }

    // Suma de (Cantidad - CantidadDisponible) de todos los recursos: lo que
    // actualmente esta en uso/movilizado, no en la base.
    public int RecursosMovilizados { get; init; }
}
