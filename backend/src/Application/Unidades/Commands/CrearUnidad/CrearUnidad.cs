using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Constants;
using Sige.Domain.Entities;
using Sige.Domain.Enums;

namespace Sige.Application.Unidades.Commands.CrearUnidad;

// FR-004: alta de unidades restringida al rol Supervisor.
[Authorize(Roles = Roles.Supervisor)]
public record CrearUnidadCommand : IRequest<int>
{
    public TipoUnidad Tipo { get; init; }

    public required string Identificador { get; init; }

    public double? Latitud { get; init; }

    public double? Longitud { get; init; }
}

public class CrearUnidadCommandHandler : IRequestHandler<CrearUnidadCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CrearUnidadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CrearUnidadCommand request, CancellationToken cancellationToken)
    {
        var entity = new UnidadRespuesta
        {
            Tipo = request.Tipo,
            Identificador = request.Identificador,
            EstadoOperativo = EstadoOperativoUnidad.Disponible,
            Latitud = request.Latitud,
            Longitud = request.Longitud
        };

        _context.UnidadesRespuesta.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
