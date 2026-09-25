namespace Sige.Domain.Entities;

// Catalogo unico compartido por Personal, UnidadRespuesta y Usuario (CHK038).
public class Institucion : BaseAuditableEntity
{
    public required string Nombre { get; set; }

    public bool Activo { get; set; } = true;
}
