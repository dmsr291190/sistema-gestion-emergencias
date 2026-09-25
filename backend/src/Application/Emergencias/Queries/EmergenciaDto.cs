using Sige.Domain.Enums;

namespace Sige.Application.Emergencias.Queries;

// FR-104: DTO de ubicacion completa para usuarios AUTENTICADOS (coordenada
// exacta, FR-115a) -- no confundir con el DTO de la vista publica (Publico/),
// que redondea la ubicacion (FR-115).
public class UbicacionDto
{
    public string? Departamento { get; init; }
    public string? Provincia { get; init; }
    public string? Distrito { get; init; }
    public string? CentroPoblado { get; init; }
    public string? Direccion { get; init; }
    public string? Referencia { get; init; }
    public double Latitud { get; init; }
    public double Longitud { get; init; }
    public Ambito Ambito { get; init; }
    public bool SinDireccionFormal { get; init; }
}

// FR-101: datos del tipo de emergencia del catalogo, embebidos en el DTO de
// emergencia para que el frontend no necesite una segunda llamada.
public class TipoEmergenciaResumenDto
{
    public int Id { get; init; }
    public required string Nombre { get; init; }
    public Ambito Ambito { get; init; }
    public string? Icono { get; init; }
    public string? Color { get; init; }
}

// FR-003, FR-011: forma usada para el mapa operativo, el listado y el dashboard.
public class EmergenciaDto
{
    public int Id { get; init; }
    public TipoEmergenciaResumenDto? TipoEmergencia { get; init; }
    public required string Descripcion { get; init; }
    public required UbicacionDto Ubicacion { get; init; }
    public int Afectados { get; init; }
    public int Heridos { get; init; }
    public int Desaparecidos { get; init; }
    public int Fallecidos { get; init; }
    public int Evacuados { get; init; }
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
