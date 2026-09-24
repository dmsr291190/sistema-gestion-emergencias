using Sige.Domain.Enums;

namespace Sige.Application.Emergencias.Queries;

// FR-003, FR-011: forma usada para el mapa operativo, el listado y el dashboard.
public class EmergenciaDto
{
    public int Id { get; init; }
    public required string Tipo { get; init; }
    public required string Descripcion { get; init; }
    public double Latitud { get; init; }
    public double Longitud { get; init; }
    public Prioridad Prioridad { get; init; }
    public EstadoEmergencia Estado { get; init; }
    public DateTimeOffset FechaHoraReporte { get; init; }
    public required string ReportanteNombre { get; init; }
    public string? ReportanteContacto { get; init; }
}

public class AsignacionDto
{
    public int Id { get; init; }
    public int UnidadId { get; init; }
    public required string UnidadIdentificador { get; init; }
    public TipoUnidad UnidadTipo { get; init; }
    public EstadoAsignacion EstadoAsignacion { get; init; }
    public DateTimeOffset FechaHoraAsignacion { get; init; }
}

public class EventoAuditoriaDto
{
    public int Id { get; init; }
    public required string TipoEvento { get; init; }
    public string? EstadoAnterior { get; init; }
    public string? EstadoNuevo { get; init; }
    public string? UsuarioId { get; init; }
    public DateTimeOffset FechaHora { get; init; }
}

// FR-008: detalle completo con asignaciones y timeline (User Story 1 y 4).
public class EmergenciaDetailDto : EmergenciaDto
{
    public List<AsignacionDto> Asignaciones { get; init; } = [];
    public List<EventoAuditoriaDto> Timeline { get; init; } = [];
}
