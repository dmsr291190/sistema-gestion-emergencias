using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;

namespace Sige.Application.Usuarios.Commands.CambiarMiPassword;

// FR-118 (Acceptance Scenario 3 de US1): cualquier usuario autenticado puede
// cambiar su propia contraseña -- usado para completar el flujo de "requiere
// cambio de contraseña" tras un ForzarCambioPassword del Administrador.
[Authorize]
public record CambiarMiPasswordCommand : IRequest
{
    public required string PasswordActual { get; init; }

    public required string PasswordNueva { get; init; }
}

public class CambiarMiPasswordCommandHandler : IRequestHandler<CambiarMiPasswordCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;

    public CambiarMiPasswordCommandHandler(IIdentityService identityService, IUser user)
    {
        _identityService = identityService;
        _user = user;
    }

    public async Task Handle(CambiarMiPasswordCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.CambiarMiPasswordAsync(_user.Id!, request.PasswordActual, request.PasswordNueva);

        if (!result.Succeeded)
        {
            throw new ConflictException("PASSWORD_NO_CAMBIADA", string.Join("; ", result.Errors));
        }
    }
}
