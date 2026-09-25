namespace Sige.Application.Emergencias.Commands.CrearEmergencia;

// FR-001, FR-015, FR-019, FR-104, FR-105, FR-125 (ampliacion 002).
// FR-021: mensajes en espanol y especificos, no el texto en ingles por defecto de
// FluentValidation, para que el frontend los muestre directamente junto al campo.
public class CrearEmergenciaCommandValidator : AbstractValidator<CrearEmergenciaCommand>
{
    public CrearEmergenciaCommandValidator()
    {
        RuleFor(v => v.TipoEmergenciaId)
            .GreaterThan(0).WithMessage("Selecciona un tipo de emergencia del catalogo.");

        RuleFor(v => v.Descripcion)
            .NotEmpty().WithMessage("La descripcion es obligatoria.")
            .MaximumLength(1000).WithMessage("La descripcion no puede superar los 1000 caracteres.");

        RuleFor(v => v.ReportanteNombre)
            .NotEmpty().WithMessage("El nombre del reportante es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar los 200 caracteres.");

        RuleFor(v => v.ReportanteContacto)
            .MaximumLength(100).WithMessage("El contacto no puede superar los 100 caracteres.");

        RuleFor(v => v.Prioridad).IsInEnum().WithMessage("Selecciona una prioridad valida.");

        RuleFor(v => v.Afectados).GreaterThanOrEqualTo(0).WithMessage("Los afectados no pueden ser negativos.");
        RuleFor(v => v.Heridos).GreaterThanOrEqualTo(0).WithMessage("Los heridos no pueden ser negativos.");
        RuleFor(v => v.Desaparecidos).GreaterThanOrEqualTo(0).WithMessage("Los desaparecidos no pueden ser negativos.");
        RuleFor(v => v.Fallecidos).GreaterThanOrEqualTo(0).WithMessage("Los fallecidos no pueden ser negativos.");
        RuleFor(v => v.Evacuados).GreaterThanOrEqualTo(0).WithMessage("Los evacuados no pueden ser negativos.");

        // data-model.md: Ubicacion.Latitud/Longitud siempre requeridas; Departamento/
        // Provincia/Distrito requeridos salvo SinDireccionFormal == true (corregido
        // tras Analyze, I2 -- no se infiere de Ambito).
        RuleFor(v => v.Ubicacion.Latitud).InclusiveBetween(-90, 90).WithMessage("La latitud debe estar entre -90 y 90.");
        RuleFor(v => v.Ubicacion.Longitud).InclusiveBetween(-180, 180).WithMessage("La longitud debe estar entre -180 y 180.");
        RuleFor(v => v.Ubicacion.Ambito).IsInEnum().WithMessage("Selecciona un ambito valido.");

        RuleFor(v => v.Ubicacion.Departamento)
            .NotEmpty().WithMessage("El departamento es obligatorio cuando la emergencia tiene direccion formal.")
            .When(v => !v.Ubicacion.SinDireccionFormal);
        RuleFor(v => v.Ubicacion.Provincia)
            .NotEmpty().WithMessage("La provincia es obligatoria cuando la emergencia tiene direccion formal.")
            .When(v => !v.Ubicacion.SinDireccionFormal);
        RuleFor(v => v.Ubicacion.Distrito)
            .NotEmpty().WithMessage("El distrito es obligatorio cuando la emergencia tiene direccion formal.")
            .When(v => !v.Ubicacion.SinDireccionFormal);
    }
}
