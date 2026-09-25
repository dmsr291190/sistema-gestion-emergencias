using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.Unidades.Queries;

// Constitution Principio V: todo acceso MUST requerir autenticacion. Hallazgo de
// Converge: esta query no tenia [Authorize] y respondia sin token.
// FR-108: filtros opcionales para el mapa avanzado (US4).
[Authorize]
public record ListarUnidadesQuery : IRequest<List<UnidadDto>>
{
    public TipoUnidad? Tipo { get; init; }
    public EstadoOperativoUnidad? EstadoOperativo { get; init; }
    public int? InstitucionId { get; init; }
}

public class ListarUnidadesQueryHandler : IRequestHandler<ListarUnidadesQuery, List<UnidadDto>>
{
    private readonly IApplicationDbContext _context;

    public ListarUnidadesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UnidadDto>> Handle(ListarUnidadesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.UnidadesRespuesta.AsQueryable();

        if (request.Tipo.HasValue) query = query.Where(u => u.Tipo == request.Tipo);
        if (request.EstadoOperativo.HasValue) query = query.Where(u => u.EstadoOperativo == request.EstadoOperativo);
        if (request.InstitucionId.HasValue) query = query.Where(u => u.InstitucionId == request.InstitucionId);

        return await query
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
