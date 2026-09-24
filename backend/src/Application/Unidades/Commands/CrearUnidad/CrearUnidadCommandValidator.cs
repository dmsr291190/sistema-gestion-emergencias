namespace Sige.Application.Unidades.Commands.CrearUnidad;

// FR-021: mensajes en espanol y especificos.
public class CrearUnidadCommandValidator : AbstractValidator<CrearUnidadCommand>
{
    public CrearUnidadCommandValidator()
    {
        RuleFor(v => v.Tipo).IsInEnum().WithMessage("Selecciona un tipo de unidad valido.");
        RuleFor(v => v.Identificador)
            .NotEmpty().WithMessage("El identificador es obligatorio.")
            .MaximumLength(50).WithMessage("El identificador no puede superar los 50 caracteres.");
    }
}
