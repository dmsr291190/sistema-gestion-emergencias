using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;

namespace Sige.Application.Usuarios.Commands.EditarUsuario;

// FR-116, FR-118: solo el rol Administrador puede editar usuarios y sus roles.
// Nota: nombre completamente calificado por el mismo motivo que CrearUsuario.cs.
[Authorize(Roles = Domain.Constants.Roles.Administrador)]
public record EditarUsuarioCommand : IRequest
{
    public required string UserId { get; init; }

    public required string NombreCompleto { get; init; }

    public int? InstitucionId { get; init; }

    public required List<string> Roles { get; init; }
}

public class EditarUsuarioCommandHandler : IRequestHandler<EditarUsuarioCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;

    public EditarUsuarioCommandHandler(IIdentityService identityService, IUser user)
    {
        _identityService = identityService;
        _user = user;
    }

    public async Task Handle(EditarUsuarioCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.EditarUsuarioAsync(
            request.UserId, request.NombreCompleto, request.InstitucionId, request.Roles, _user.Id);

        if (!result.Succeeded)
        {
            throw new ConflictException("USUARIO_NO_EDITADO", string.Join("; ", result.Errors));
        }
    }
}
