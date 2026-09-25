namespace Sige.Domain.Entities;

// FR-101: catalogo administrable que reemplaza el texto libre Emergencia.Tipo del MVP.
public class TipoEmergencia : BaseAuditableEntity
{
    public required string Nombre { get; set; }

    public Ambito Ambito { get; set; }

    public string? Icono { get; set; }

    public string? Color { get; set; }

    public Prioridad PrioridadPorDefecto { get; set; }

    // FR-102: desactivar no borra ni afecta emergencias existentes que ya lo usan.
    public bool Activo { get; set; } = true;
}
