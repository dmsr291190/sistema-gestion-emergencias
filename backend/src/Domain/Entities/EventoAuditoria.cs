namespace Sige.Domain.Entities;

// FR-008, FR-010: fuente de la linea de tiempo mostrada en el detalle de la emergencia.
public class EventoAuditoria : BaseEntity
{
    public int? EmergenciaId { get; set; }

    public Emergencia? Emergencia { get; set; }

    public int? UnidadId { get; set; }

    public UnidadRespuesta? Unidad { get; set; }

    // Ej. "EmergenciaCreada", "EmergenciaValidada", "UnidadAsignada", "CambioEstado",
    // "EmergenciaCerrada", "EmergenciaReabierta".
    public required string TipoEvento { get; set; }

    public string? EstadoAnterior { get; set; }

    public string? EstadoNuevo { get; set; }

    public string? UsuarioId { get; set; }

    public DateTimeOffset FechaHora { get; set; }
}
