using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.PersonalUnidades.Queries;

public class PersonalDto
{
    public int Id { get; init; }
    public required string Nombres { get; init; }
    public required string Apellidos { get; init; }
    public required string Documento { get; init; }
    public int? InstitucionId { get; init; }
    public string? Especialidad { get; init; }
    public string? Funcion { get; init; }
    public string? Certificaciones { get; init; }
    public bool Disponible { get; init; }
    public int UnidadRespuestaId { get; init; }
}

// FR-110: personal visible al seleccionar la unidad en despacho o mapa.
// FR-120a (hallazgo CRITICAL de Converge): el Visualizador MUST NOT tener
// acceso a personal -- se listan explicitamente los roles permitidos en vez de
// [Authorize] generico, que dejaba pasar a cualquier rol autenticado.
[Authorize(Roles = $"{Roles.Operador},{Roles.Supervisor},{Roles.Administrador},{Roles.CoordinadorLogistico},{Roles.JefeDeUnidad},{Roles.UnidadDeRespuesta}")]
public record ListarPersonalPorUnidadQuery : IRequest<List<PersonalDto>>
{
    public required int UnidadRespuestaId { get; init; }
}

public class ListarPersonalPorUnidadQueryHandler : IRequestHandler<ListarPersonalPorUnidadQuery, List<PersonalDto>>
{
    private readonly IApplicationDbContext _context;

    public ListarPersonalPorUnidadQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PersonalDto>> Handle(ListarPersonalPorUnidadQuery request, CancellationToken cancellationToken)
    {
        return await _context.Personal
            .Where(p => p.UnidadRespuestaId == request.UnidadRespuestaId)
            .OrderBy(p => p.Apellidos)
            .Select(p => new PersonalDto
            {
                Id = p.Id,
                Nombres = p.Nombres,
                Apellidos = p.Apellidos,
                Documento = p.Documento,
                InstitucionId = p.InstitucionId,
                Especialidad = p.Especialidad,
                Funcion = p.Funcion,
                Certificaciones = p.Certificaciones,
                Disponible = p.Disponible,
                UnidadRespuestaId = p.UnidadRespuestaId
            })
            .ToListAsync(cancellationToken);
    }
}
