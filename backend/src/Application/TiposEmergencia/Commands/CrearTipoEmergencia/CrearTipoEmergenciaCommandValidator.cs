namespace Sige.Application.TiposEmergencia.Commands.CrearTipoEmergencia;

// FR-021: mensajes en espanol y especificos. data-model.md: Nombre requerido,
// unico, max 100 caracteres.
public class CrearTipoEmergenciaCommandValidator : AbstractValidator<CrearTipoEmergenciaCommand>
{
    public CrearTipoEmergenciaCommandValidator()
    {
        RuleFor(v => v.Nombre)
            .NotEmpty().WithMessage("El nombre del tipo de emergencia es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(v => v.Ambito).IsInEnum().WithMessage("Selecciona un ámbito válido.");
        RuleFor(v => v.PrioridadPorDefecto).IsInEnum().WithMessage("Selecciona una prioridad válida.");
    }
}
