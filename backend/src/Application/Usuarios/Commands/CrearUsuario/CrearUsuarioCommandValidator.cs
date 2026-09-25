namespace Sige.Application.Usuarios.Commands.CrearUsuario;

// FR-021: mensajes en espanol y especificos.
public class CrearUsuarioCommandValidator : AbstractValidator<CrearUsuarioCommand>
{
    public CrearUsuarioCommandValidator()
    {
        RuleFor(v => v.UserName)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .MaximumLength(256).WithMessage("El nombre de usuario no puede superar los 256 caracteres.");

        RuleFor(v => v.NombreCompleto)
            .NotEmpty().WithMessage("El nombre completo es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre completo no puede superar los 200 caracteres.");

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");

        RuleFor(v => v.Roles)
            .NotEmpty().WithMessage("Selecciona al menos un rol.");

        RuleForEach(v => v.Roles)
            .Must(rol => Domain.Constants.Roles.Todos.Contains(rol))
            .WithMessage("Uno de los roles seleccionados no es válido.");
    }
}
