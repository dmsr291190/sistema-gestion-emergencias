namespace Sige.Application.Emergencias.Commands.CrearEmergencia;

// FR-001, FR-015, FR-019, Assumptions (ubicacion obligatoria).
public class CrearEmergenciaCommandValidator : AbstractValidator<CrearEmergenciaCommand>
{
    public CrearEmergenciaCommandValidator()
    {
        RuleFor(v => v.Tipo).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Descripcion).NotEmpty().MaximumLength(1000);
        RuleFor(v => v.ReportanteNombre).NotEmpty().MaximumLength(200);
        RuleFor(v => v.ReportanteContacto).MaximumLength(100);
        RuleFor(v => v.Prioridad).IsInEnum();
        RuleFor(v => v.Latitud).InclusiveBetween(-90, 90);
        RuleFor(v => v.Longitud).InclusiveBetween(-180, 180);
    }
}
