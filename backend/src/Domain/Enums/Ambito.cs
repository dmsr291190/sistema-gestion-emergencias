namespace Sige.Domain.Enums;

// FR-105: ambito de una emergencia o tipo de emergencia. Ubicacion.Ambito y
// TipoEmergencia.Ambito comparten estos 4 valores (corregido tras Analyze, I1).
public enum Ambito
{
    Terrestre = 0,
    Maritimo = 1,
    Aereo = 2,
    Mixto = 3
}
