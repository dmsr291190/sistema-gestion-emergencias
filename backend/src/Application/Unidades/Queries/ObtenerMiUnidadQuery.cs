using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.Unidades.Queries;

// FR-120: la pantalla "mi unidad" del rol UnidadDeRespuesta solo debe poder ver
// su propia unidad, no el listado completo (GET /api/Unidades es para
// Operador/Supervisor/etc.).
[Authorize(Roles = Roles.UnidadDeRespuesta)]
public record ObtenerMiUnidadQuery : IRequest<UnidadDto?>;

public class ObtenerMiUnidadQueryHandler : IRequestHandler<ObtenerMiUnidadQuery, UnidadDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public ObtenerMiUnidadQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<UnidadDto?> Handle(ObtenerMiUnidadQuery request, CancellationToken cancellationToken)
    {
        return await _context.UnidadesRespuesta
            .Where(u => u.UsuarioId == _user.Id)
            .Select(u => new UnidadDto
            {
                Id = u.Id,
                Tipo = u.Tipo,
                Identificador = u.Identificador,
                EstadoOperativo = u.EstadoOperativo,
                Latitud = u.Latitud,
                Longitud = u.Longitud
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
