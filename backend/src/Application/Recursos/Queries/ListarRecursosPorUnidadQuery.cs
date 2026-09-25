using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Constants;
using Sige.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.Recursos.Queries;

public class RecursoDto
{
    public int Id { get; init; }
    public required string Codigo { get; init; }
    public required string Nombre { get; init; }
    public CategoriaRecurso Categoria { get; init; }
    public string? UnidadMedida { get; init; }
    public int Cantidad { get; init; }
    public int CantidadDisponible { get; init; }
    public int CantidadMinima { get; init; }
    public int UnidadRespuestaId { get; init; }

    // FR-112: se marca cuando la cantidad disponible cae por debajo del mínimo.
    public bool BajoStock => CantidadDisponible < CantidadMinima;
}

// FR-120a (hallazgo CRITICAL de Converge): el Visualizador MUST NOT tener
// acceso a recursos -- mismo fix que ListarPersonalPorUnidadQuery.
[Authorize(Roles = $"{Roles.Operador},{Roles.Supervisor},{Roles.Administrador},{Roles.CoordinadorLogistico},{Roles.JefeDeUnidad},{Roles.UnidadDeRespuesta}")]
public record ListarRecursosPorUnidadQuery : IRequest<List<RecursoDto>>
{
    public required int UnidadRespuestaId { get; init; }
}

public class ListarRecursosPorUnidadQueryHandler : IRequestHandler<ListarRecursosPorUnidadQuery, List<RecursoDto>>
{
    private readonly IApplicationDbContext _context;

    public ListarRecursosPorUnidadQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecursoDto>> Handle(ListarRecursosPorUnidadQuery request, CancellationToken cancellationToken)
    {
        return await _context.Recursos
            .Where(r => r.UnidadRespuestaId == request.UnidadRespuestaId)
            .OrderBy(r => r.Nombre)
            .Select(r => new RecursoDto
            {
                Id = r.Id,
                Codigo = r.Codigo,
                Nombre = r.Nombre,
                Categoria = r.Categoria,
                UnidadMedida = r.UnidadMedida,
                Cantidad = r.Cantidad,
                CantidadDisponible = r.CantidadDisponible,
                CantidadMinima = r.CantidadMinima,
                UnidadRespuestaId = r.UnidadRespuestaId
            })
            .ToListAsync(cancellationToken);
    }
}
