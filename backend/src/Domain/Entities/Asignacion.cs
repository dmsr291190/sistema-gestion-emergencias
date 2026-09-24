namespace Sige.Domain.Entities;

public class Asignacion : BaseEntity
{
    public int EmergenciaId { get; set; }

    public Emergencia? Emergencia { get; set; }

    public int UnidadId { get; set; }

    public UnidadRespuesta? Unidad { get; set; }

    // FR-016: progreso independiente por unidad dentro de la misma emergencia
    public EstadoAsignacion EstadoAsignacion { get; set; } = EstadoAsignacion.Despachada;

    public string? AsignadoPorId { get; set; }

    public DateTimeOffset FechaHoraAsignacion { get; set; }
}
