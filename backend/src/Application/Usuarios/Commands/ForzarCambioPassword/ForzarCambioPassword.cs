using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Constants;

namespace Sige.Application.Usuarios.Commands.ForzarCambioPassword;

// FR-116, FR-118: solo el rol Administrador puede forzar el restablecimiento de
// la contraseña de otro usuario.
[Authorize(Roles = Roles.Administrador)]
public record ForzarCambioPasswordCommand : IRequest
{
    public required string UserId { get; init; }

    public required string PasswordTemporal { get; init; }
}

public class ForzarCambioPasswordCommandHandler : IRequestHandler<ForzarCambioPasswordCommand>
{
    private readonly IIdentityService _identityService;

    public ForzarCambioPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task Handle(ForzarCambioPasswordCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.ForzarCambioPasswordAsync(request.UserId, request.PasswordTemporal);

        if (!result.Succeeded)
        {
            throw new ConflictException("PASSWORD_NO_RESTABLECIDA", string.Join("; ", result.Errors));
        }
    }
}
