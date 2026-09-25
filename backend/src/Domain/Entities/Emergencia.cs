namespace Sige.Domain.Entities;

public class Emergencia : BaseAuditableEntity
{
    // Columna legada del MVP (texto libre); se conserva para trazabilidad, no se
    // borra (Principio IV), pero ya no es la fuente de verdad del tipo — ver
    // TipoEmergenciaId (FR-103).
    public string? Tipo { get; set; }

    public int? TipoEmergenciaId { get; set; }

    public TipoEmergencia? TipoEmergencia { get; set; }

    public required string Descripcion { get; set; }

    // FR-104: reemplaza los campos sueltos Latitud/Longitud del MVP (ver
    // migracion de datos en ApplicationDbContextInitialiser).
    public Ubicacion Ubicacion { get; set; } = new();

    // FR-125: formulario enriquecido de la ampliacion.
    public int Afectados { get; set; }

    public int Heridos { get; set; }

    public int Desaparecidos { get; set; }

    public int Fallecidos { get; set; }

    public int Evacuados { get; set; }

    public Prioridad Prioridad { get; set; }

    public DateTimeOffset FechaHoraReporte { get; set; }

    // FR-019: nombre obligatorio, contacto opcional
    public required string ReportanteNombre { get; set; }

    public string? ReportanteContacto { get; set; }

    public EstadoEmergencia Estado { get; set; } = EstadoEmergencia.Reportada;

    public string? CreadoPorId { get; set; }

    public IList<Asignacion> Asignaciones { get; private set; } = new List<Asignacion>();

    public IList<EventoAuditoria> EventosAuditoria { get; private set; } = new List<EventoAuditoria>();
}
