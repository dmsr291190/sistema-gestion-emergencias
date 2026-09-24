using Sige.Domain.Enums;

namespace Sige.Application.Unidades.Queries;

// FR-004, FR-005: disponibilidad y estado operativo de cada unidad.
public class UnidadDto
{
    public int Id { get; init; }
    public TipoUnidad Tipo { get; init; }
    public required string Identificador { get; init; }
    public EstadoOperativoUnidad EstadoOperativo { get; init; }
    public double? Latitud { get; init; }
    public double? Longitud { get; init; }
}
