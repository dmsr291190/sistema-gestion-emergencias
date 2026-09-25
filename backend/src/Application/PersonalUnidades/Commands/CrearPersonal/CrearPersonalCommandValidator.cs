namespace Sige.Application.PersonalUnidades.Commands.CrearPersonal;

// FR-021: mensajes en espanol. data-model.md: Documento requerido.
public class CrearPersonalCommandValidator : AbstractValidator<CrearPersonalCommand>
{
    public CrearPersonalCommandValidator()
    {
        RuleFor(v => v.Nombres).NotEmpty().WithMessage("Los nombres son obligatorios.").MaximumLength(150);
        RuleFor(v => v.Apellidos).NotEmpty().WithMessage("Los apellidos son obligatorios.").MaximumLength(150);
        RuleFor(v => v.Documento)
            .NotEmpty().WithMessage("El documento es obligatorio.")
            .MaximumLength(20).WithMessage("El documento no puede superar los 20 caracteres.");
        RuleFor(v => v.UnidadRespuestaId).GreaterThan(0).WithMessage("Selecciona una unidad válida.");
    }
}
