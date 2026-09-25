using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.Unidades.Queries;

// US5: detalle de una unidad (personal/recursos se consultan aparte, endpoints anidados).
[Authorize]
public record ObtenerUnidadPorIdQuery : IRequest<UnidadDto?>
{
    public required int UnidadId { get; init; }
}

public class ObtenerUnidadPorIdQueryHandler : IRequestHandler<ObtenerUnidadPorIdQuery, UnidadDto?>
{
    private readonly IApplicationDbContext _context;

    public ObtenerUnidadPorIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UnidadDto?> Handle(ObtenerUnidadPorIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.UnidadesRespuesta
            .Where(u => u.Id == request.UnidadId)
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
