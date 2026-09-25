using Sige.Domain.Enums;

namespace Sige.Domain.ValueObjects;

// FR-104: owned type reutilizado en Emergencia y UnidadRespuesta. Corregido tras
// Analyze (I1: Ambito con 4 valores; I2: SinDireccionFormal en vez de inferir la
// obligatoriedad de Departamento/Provincia/Distrito a partir de Ambito).
public class Ubicacion
{
    public string? Departamento { get; set; }

    public string? Provincia { get; set; }

    public string? Distrito { get; set; }

    public string? CentroPoblado { get; set; }

    public string? Direccion { get; set; }

    public string? Referencia { get; set; }

    public double Latitud { get; set; }

    public double Longitud { get; set; }

    public Ambito Ambito { get; set; }

    // FR-104: si es true, Departamento/Provincia/Distrito pueden quedar null
    // (zonas maritimas o terrestres remotas sin direccion formal).
    public bool SinDireccionFormal { get; set; }
}
