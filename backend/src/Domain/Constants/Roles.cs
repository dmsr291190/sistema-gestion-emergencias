namespace Sige.Domain.Constants;

public abstract class Roles
{
    // FR-012: Operador y Supervisor son los dos roles del MVP.
    public const string Operador = nameof(Operador);

    public const string Supervisor = nameof(Supervisor);
}