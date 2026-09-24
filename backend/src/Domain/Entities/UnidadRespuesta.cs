namespace Sige.Domain.Entities;

public class UnidadRespuesta : BaseAuditableEntity
{
    public TipoUnidad Tipo { get; set; }

    public required string Identificador { get; set; }

    public EstadoOperativoUnidad EstadoOperativo { get; set; } = EstadoOperativoUnidad.Disponible;

    public double? Latitud { get; set; }

    public double? Longitud { get; set; }

    public IList<Asignacion> Asignaciones { get; private set; } = new List<Asignacion>();
}
