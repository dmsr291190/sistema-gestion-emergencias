namespace Sige.Application.Recursos.Commands.CrearRecurso;

// FR-021: mensajes en espanol.
// data-model.md (regla citada explicitamente tras /speckit-analyze, hallazgo U1):
// Recurso.CantidadDisponible <= Recurso.Cantidad.
public class CrearRecursoCommandValidator : AbstractValidator<CrearRecursoCommand>
{
    public CrearRecursoCommandValidator()
    {
        RuleFor(v => v.UnidadRespuestaId).GreaterThan(0).WithMessage("Selecciona una unidad válida.");
        RuleFor(v => v.Codigo).NotEmpty().WithMessage("El código es obligatorio.").MaximumLength(50);
        RuleFor(v => v.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.").MaximumLength(150);
        RuleFor(v => v.Categoria).IsInEnum().WithMessage("Selecciona una categoría válida.");
        RuleFor(v => v.Cantidad).GreaterThanOrEqualTo(0).WithMessage("La cantidad no puede ser negativa.");
        RuleFor(v => v.CantidadMinima).GreaterThanOrEqualTo(0).WithMessage("La cantidad mínima no puede ser negativa.");
        RuleFor(v => v.CantidadDisponible)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad disponible no puede ser negativa.")
            .LessThanOrEqualTo(v => v.Cantidad).WithMessage("La cantidad disponible no puede superar la cantidad total.");
    }
}
