namespace Sige.Domain.Entities;

public class UnidadRespuesta : BaseAuditableEntity
{
    public TipoUnidad Tipo { get; set; }

    public required string Identificador { get; set; }

    public EstadoOperativoUnidad EstadoOperativo { get; set; } = EstadoOperativoUnidad.Disponible;

    public double? Latitud { get; set; }

    public double? Longitud { get; set; }

    // FR-104: ubicacion completa de la ampliacion (departamento/provincia/etc);
    // Latitud/Longitud de arriba se conservan por compatibilidad con el MVP.
    public Ubicacion? Ubicacion { get; set; }

    public int? InstitucionId { get; set; }

    public Institucion? Institucion { get; set; }

    // FR-120: vinculo 1-a-1 con la cuenta de login propia cuando el rol es
    // "Unidad de respuesta" (research.md §2).
    public string? UsuarioId { get; set; }

    public IList<Asignacion> Asignaciones { get; private set; } = new List<Asignacion>();

    public IList<Personal> Personal { get; private set; } = new List<Personal>();

    public IList<Recurso> Recursos { get; private set; } = new List<Recurso>();
}
