using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.Unidades.Queries;

// Constitution Principio V: todo acceso MUST requerir autenticacion. Hallazgo de
// Converge: esta query no tenia [Authorize] y respondia sin token.
[Authorize]
public record ListarUnidadesQuery : IRequest<List<UnidadDto>>;

public class ListarUnidadesQueryHandler : IRequestHandler<ListarUnidadesQuery, List<UnidadDto>>
{
    private readonly IApplicationDbContext _context;

    public ListarUnidadesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UnidadDto>> Handle(ListarUnidadesQuery request, CancellationToken cancellationToken)
    {
        return await _context.UnidadesRespuesta
            .OrderBy(u => u.Tipo).ThenBy(u => u.Identificador)
            .Select(u => new UnidadDto
            {
                Id = u.Id,
                Tipo = u.Tipo,
                Identificador = u.Identificador,
                EstadoOperativo = u.EstadoOperativo,
                Latitud = u.Latitud,
                Longitud = u.Longitud
            })
            .ToListAsync(cancellationToken);
    }
}
