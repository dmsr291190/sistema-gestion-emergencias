using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Constants;

namespace Sige.Application.Usuarios.Commands.DesbloquearUsuario;

// FR-116, FR-118: solo el rol Administrador puede desbloquear una cuenta.
[Authorize(Roles = Roles.Administrador)]
public record DesbloquearUsuarioCommand : IRequest
{
    public required string UserId { get; init; }
}

public class DesbloquearUsuarioCommandHandler : IRequestHandler<DesbloquearUsuarioCommand>
{
    private readonly IIdentityService _identityService;

    public DesbloquearUsuarioCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task Handle(DesbloquearUsuarioCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.DesbloquearUsuarioAsync(request.UserId);

        if (!result.Succeeded)
        {
            throw new ConflictException("USUARIO_NO_DESBLOQUEADO", string.Join("; ", result.Errors));
        }
    }
}
