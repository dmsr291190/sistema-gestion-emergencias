namespace Sige.Domain.Enums;

// FR-016: progreso independiente por unidad dentro de una misma emergencia
public enum EstadoAsignacion
{
    Despachada = 0,
    EnRuta = 1,
    EnElLugar = 2,
    Atendida = 3
}
