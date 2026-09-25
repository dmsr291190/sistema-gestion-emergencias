namespace Sige.Domain.Constants;

public abstract class Roles
{
    // FR-012: Operador y Supervisor son los dos roles del MVP.
    public const string Operador = nameof(Operador);

    public const string Supervisor = nameof(Supervisor);

    // FR-117: roles nuevos de la ampliacion, estrictamente aditivos.
    public const string Administrador = nameof(Administrador);

    public const string CoordinadorLogistico = nameof(CoordinadorLogistico);

    public const string JefeDeUnidad = nameof(JefeDeUnidad);

    public const string UnidadDeRespuesta = nameof(UnidadDeRespuesta);

    public const string Visualizador = nameof(Visualizador);

    public static readonly string[] Todos =
    [
        Operador, Supervisor, Administrador, CoordinadorLogistico, JefeDeUnidad, UnidadDeRespuesta, Visualizador
    ];
}