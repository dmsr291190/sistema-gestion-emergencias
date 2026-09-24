namespace Sige.Domain.Enums;

// FR-009: reportada -> validada -> despachada -> en ruta -> en el lugar -> atendida -> cerrada
public enum EstadoEmergencia
{
    Reportada = 0,
    Validada = 1,
    Despachada = 2,
    EnRuta = 3,
    EnElLugar = 4,
    Atendida = 5,
    Cerrada = 6
}
