using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.TiposEmergencia.Queries;

public class TipoEmergenciaDto
{
    public int Id { get; init; }
    public required string Nombre { get; init; }
    public Ambito Ambito { get; init; }
    public string? Icono { get; init; }
    public string? Color { get; init; }
    public Prioridad PrioridadPorDefecto { get; init; }
    public bool Activo { get; init; }
}

// FR-102: ?soloActivos=true para el formulario de registro de emergencias, que
// no debe ofrecer tipos desactivados.
[Authorize]
public record ListarTiposEmergenciaQuery : IRequest<List<TipoEmergenciaDto>>
{
    public bool SoloActivos { get; init; }
}

public class ListarTiposEmergenciaQueryHandler : IRequestHandler<ListarTiposEmergenciaQuery, List<TipoEmergenciaDto>>
{
    private readonly IApplicationDbContext _context;

    public ListarTiposEmergenciaQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TipoEmergenciaDto>> Handle(ListarTiposEmergenciaQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TiposEmergencia.AsQueryable();

        if (request.SoloActivos)
        {
            query = query.Where(t => t.Activo);
        }

        return await query
            .OrderBy(t => t.Nombre)
            .Select(t => new TipoEmergenciaDto
            {
                Id = t.Id,
                Nombre = t.Nombre,
                Ambito = t.Ambito,
                Icono = t.Icono,
                Color = t.Color,
                PrioridadPorDefecto = t.PrioridadPorDefecto,
                Activo = t.Activo
            })
            .ToListAsync(cancellationToken);
    }
}
