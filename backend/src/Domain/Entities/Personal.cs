namespace Sige.Domain.Entities;

// FR-110: personal asociado a una unidad de respuesta.
public class Personal : BaseAuditableEntity
{
    public required string Nombres { get; set; }

    public required string Apellidos { get; set; }

    public required string Documento { get; set; }

    public int? InstitucionId { get; set; }

    public Institucion? Institucion { get; set; }

    public string? Especialidad { get; set; }

    public string? Funcion { get; set; }

    public string? Certificaciones { get; set; }

    public bool Disponible { get; set; } = true;

    public int UnidadRespuestaId { get; set; }

    public UnidadRespuesta UnidadRespuesta { get; set; } = null!;
}
