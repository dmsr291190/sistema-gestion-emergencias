using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Constants;
using Sige.Domain.Entities;
using Sige.Domain.Enums;

namespace Sige.Application.Recursos.Commands.CrearRecurso;

// FR-111: recursos transportados por una unidad.
[Authorize(Roles = $"{Roles.CoordinadorLogistico},{Roles.JefeDeUnidad}")]
public record CrearRecursoCommand : IRequest<int>
{
    public required int UnidadRespuestaId { get; init; }

    public required string Codigo { get; init; }

    public required string Nombre { get; init; }

    public CategoriaRecurso Categoria { get; init; }

    public string? UnidadMedida { get; init; }

    public int Cantidad { get; init; }

    public int CantidadDisponible { get; init; }

    public int CantidadMinima { get; init; }
}

public class CrearRecursoCommandHandler : IRequestHandler<CrearRecursoCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CrearRecursoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CrearRecursoCommand request, CancellationToken cancellationToken)
    {
        _ = await _context.UnidadesRespuesta.FindAsync([request.UnidadRespuestaId], cancellationToken)
            ?? throw new NotFoundException(nameof(UnidadRespuesta), request.UnidadRespuestaId.ToString());

        var entity = new Recurso
        {
            Codigo = request.Codigo,
            Nombre = request.Nombre,
            Categoria = request.Categoria,
            UnidadMedida = request.UnidadMedida,
            Cantidad = request.Cantidad,
            CantidadDisponible = request.CantidadDisponible,
            CantidadMinima = request.CantidadMinima,
            UnidadRespuestaId = request.UnidadRespuestaId
        };

        _context.Recursos.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
