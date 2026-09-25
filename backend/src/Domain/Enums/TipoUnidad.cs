namespace Sige.Domain.Enums;

public enum TipoUnidad
{
    Ambulancia = 0,
    Bomberos = 1,
    Patrullero = 2,

    // Sección 6.2 del documento de origen (ampliación 002) — valores nuevos
    // agregados al final para no romper los índices 0-2 ya persistidos del MVP.
    VehiculoRescate = 3,
    CamionLogistico = 4,
    Embarcacion = 5,
    Helicoptero = 6,
    PuestoDeComando = 7
}
