using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Constants;

namespace Sige.Application.Usuarios.Queries;

// FR-116, FR-119: solo el rol Administrador puede listar usuarios con su
// historial de seguridad (ultimo acceso, intentos fallidos).
[Authorize(Roles = Roles.Administrador)]
public record ListarUsuariosQuery : IRequest<List<UsuarioAdminDto>>;

public class ListarUsuariosQueryHandler : IRequestHandler<ListarUsuariosQuery, List<UsuarioAdminDto>>
{
    private readonly IIdentityService _identityService;

    public ListarUsuariosQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<List<UsuarioAdminDto>> Handle(ListarUsuariosQuery request, CancellationToken cancellationToken)
    {
        return _identityService.ListarUsuariosAsync();
    }
}
