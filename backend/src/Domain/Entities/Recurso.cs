namespace Sige.Domain.Entities;

// FR-111, FR-112: recursos transportados por una unidad de respuesta.
public class Recurso : BaseAuditableEntity
{
    public required string Codigo { get; set; }

    public required string Nombre { get; set; }

    public CategoriaRecurso Categoria { get; set; }

    public string? UnidadMedida { get; set; }

    public int Cantidad { get; set; }

    // FR-112: se marca bajoStock cuando CantidadDisponible < CantidadMinima.
    public int CantidadDisponible { get; set; }

    public int CantidadMinima { get; set; }

    public int UnidadRespuestaId { get; set; }

    public UnidadRespuesta UnidadRespuesta { get; set; } = null!;
}
