using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Constants;

namespace Sige.Application.Usuarios.Commands.BloquearUsuario;

// FR-116, FR-118: solo el rol Administrador puede bloquear una cuenta.
[Authorize(Roles = Roles.Administrador)]
public record BloquearUsuarioCommand : IRequest
{
    public required string UserId { get; init; }
}

public class BloquearUsuarioCommandHandler : IRequestHandler<BloquearUsuarioCommand>
{
    private readonly IIdentityService _identityService;

    public BloquearUsuarioCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task Handle(BloquearUsuarioCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.BloquearUsuarioAsync(request.UserId);

        if (!result.Succeeded)
        {
            throw new ConflictException("USUARIO_NO_BLOQUEADO", string.Join("; ", result.Errors));
        }
    }
}
