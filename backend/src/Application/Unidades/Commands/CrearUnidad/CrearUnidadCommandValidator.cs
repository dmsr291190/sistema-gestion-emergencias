namespace Sige.Application.Unidades.Commands.CrearUnidad;

public class CrearUnidadCommandValidator : AbstractValidator<CrearUnidadCommand>
{
    public CrearUnidadCommandValidator()
    {
        RuleFor(v => v.Tipo).IsInEnum();
        RuleFor(v => v.Identificador).NotEmpty().MaximumLength(50);
    }
}
