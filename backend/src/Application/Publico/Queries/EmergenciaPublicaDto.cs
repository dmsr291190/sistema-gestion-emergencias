using Sige.Domain.Enums;

namespace Sige.Application.Publico.Queries;

// FR-114: DTOs propios, deliberadamente SEPARADOS de EmergenciaDto (Application/
// Emergencias/Queries) -- nunca deben incluir documento, telefono, correo,
// nombres de reportantes/victimas, informacion medica, coordenadas exactas,
// observaciones internas ni datos de auditoria interna. La separacion de tipos
// hace estructuralmente imposible que un cambio futuro filtre un campo sensible
// por accidente (el mismo tipo de bug que se encontro una vez en el MVP con la
// serializacion de ValidationProblemDetails).
public class UbicacionPublicaDto
{
    public string? Departamento { get; init; }
    public string? Provincia { get; init; }
    public string? Distrito { get; init; }
    public string? CentroPoblado { get; init; }

    // FR-115: aproximada (redondeada), nunca la coordenada exacta reportada.
    public double LatitudAproximada { get; init; }
    public double LongitudAproximada { get; init; }
    public Ambito Ambito { get; init; }
}

public class EmergenciaPublicaDto
{
    public required int Codigo { get; init; }
    public string? TipoNombre { get; init; }
    public string? TipoIcono { get; init; }
    public string? TipoColor { get; init; }
    public Prioridad Prioridad { get; init; }
    public EstadoEmergencia Estado { get; init; }
    public required UbicacionPublicaDto Ubicacion { get; init; }
    public DateTimeOffset UltimaActualizacion { get; init; }
}

public class EventoPublicoDto
{
    public required string TipoEvento { get; init; }
    public string? EstadoNuevo { get; init; }
    public DateTimeOffset FechaHora { get; init; }
}

public class EmergenciaPublicaDetalleDto : EmergenciaPublicaDto
{
    public List<EventoPublicoDto> Timeline { get; init; } = [];
}
