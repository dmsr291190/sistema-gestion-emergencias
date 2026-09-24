namespace Sige.Application.Common.Exceptions;

// Mapeada a 409 Conflict por ProblemDetailsExceptionHandler. Usada para rechazos de
// negocio como UNIDAD_NO_DISPONIBLE (FR-007) — ver contracts/rest-api.md.
public class ConflictException : Exception
{
    public string Codigo { get; }

    public ConflictException(string codigo, string message) : base(message)
    {
        Codigo = codigo;
    }
}
