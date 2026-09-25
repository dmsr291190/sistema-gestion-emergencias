using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;

namespace Sige.Application.Usuarios.Commands.CrearUsuario;

// FR-116, FR-118: solo el rol Administrador puede crear usuarios.
// Nota: se usa el nombre completamente calificado Domain.Constants.Roles porque
// este record ya tiene su propia propiedad "Roles" (los roles asignados al nuevo
// usuario), que de otro modo eclipsaria el nombre de la clase de constantes.
[Authorize(Roles = Domain.Constants.Roles.Administrador)]
public record CrearUsuarioCommand : IRequest<string>
{
    public required string UserName { get; init; }

    public required string NombreCompleto { get; init; }

    public required string Password { get; init; }

    public int? InstitucionId { get; init; }

    public required List<string> Roles { get; init; }
}

public class CrearUsuarioCommandHandler : IRequestHandler<CrearUsuarioCommand, string>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;

    public CrearUsuarioCommandHandler(IIdentityService identityService, IUser user)
    {
        _identityService = identityService;
        _user = user;
    }

    public async Task<string> Handle(CrearUsuarioCommand request, CancellationToken cancellationToken)
    {
        var (result, userId) = await _identityService.CrearUsuarioAsync(
            request.UserName, request.NombreCompleto, request.Password, request.InstitucionId, request.Roles, _user.Id);

        if (!result.Succeeded)
        {
            throw new ConflictException("USUARIO_NO_CREADO", string.Join("; ", result.Errors));
        }

        return userId;
    }
}
