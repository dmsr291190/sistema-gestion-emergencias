namespace Sige.Domain.Entities;

public class Emergencia : BaseAuditableEntity
{
    public required string Tipo { get; set; }

    public required string Descripcion { get; set; }

    public double Latitud { get; set; }

    public double Longitud { get; set; }

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
