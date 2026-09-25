using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.Instituciones.Queries;

public class InstitucionDto
{
    public int Id { get; init; }
    public required string Nombre { get; init; }
    public bool Activo { get; init; }
}

// Agregado tras /speckit-analyze (hallazgo C2): Personal, UnidadRespuesta y
// Usuario referencian InstitucionId sin tener de donde leer el catalogo.
[Authorize]
public record ListarInstitucionesQuery : IRequest<List<InstitucionDto>>;

public class ListarInstitucionesQueryHandler : IRequestHandler<ListarInstitucionesQuery, List<InstitucionDto>>
{
    private readonly IApplicationDbContext _context;

    public ListarInstitucionesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<InstitucionDto>> Handle(ListarInstitucionesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Instituciones
            .OrderBy(i => i.Nombre)
            .Select(i => new InstitucionDto { Id = i.Id, Nombre = i.Nombre, Activo = i.Activo })
            .ToListAsync(cancellationToken);
    }
}
