namespace Sige.Application.Emergencias.Commands.CrearEmergencia;

// FR-001, FR-015, FR-019, Assumptions (ubicacion obligatoria).
// FR-021: mensajes en espanol y especificos, no el texto en ingles por defecto de
// FluentValidation, para que el frontend los muestre directamente junto al campo.
public class CrearEmergenciaCommandValidator : AbstractValidator<CrearEmergenciaCommand>
{
    public CrearEmergenciaCommandValidator()
    {
        RuleFor(v => v.Tipo)
            .NotEmpty().WithMessage("El tipo de emergencia es obligatorio.")
            .MaximumLength(100).WithMessage("El tipo no puede superar los 100 caracteres.");

        RuleFor(v => v.Descripcion)
            .NotEmpty().WithMessage("La descripcion es obligatoria.")
            .MaximumLength(1000).WithMessage("La descripcion no puede superar los 1000 caracteres.");

        RuleFor(v => v.ReportanteNombre)
            .NotEmpty().WithMessage("El nombre del reportante es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar los 200 caracteres.");

        RuleFor(v => v.ReportanteContacto)
            .MaximumLength(100).WithMessage("El contacto no puede superar los 100 caracteres.");

        RuleFor(v => v.Prioridad).IsInEnum().WithMessage("Selecciona una prioridad valida.");

        RuleFor(v => v.Latitud).InclusiveBetween(-90, 90).WithMessage("La latitud debe estar entre -90 y 90.");
        RuleFor(v => v.Longitud).InclusiveBetween(-180, 180).WithMessage("La longitud debe estar entre -180 y 180.");
    }
}
