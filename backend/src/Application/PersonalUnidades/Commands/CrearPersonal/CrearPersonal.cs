using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Constants;
using Sige.Domain.Entities;

namespace Sige.Application.PersonalUnidades.Commands.CrearPersonal;

// FR-110: personal asociado a una unidad de respuesta.
[Authorize(Roles = $"{Roles.CoordinadorLogistico},{Roles.JefeDeUnidad}")]
public record CrearPersonalCommand : IRequest<int>
{
    public required int UnidadRespuestaId { get; init; }

    public required string Nombres { get; init; }

    public required string Apellidos { get; init; }

    public required string Documento { get; init; }

    public int? InstitucionId { get; init; }

    public string? Especialidad { get; init; }

    public string? Funcion { get; init; }

    public string? Certificaciones { get; init; }

    public bool Disponible { get; init; } = true;
}

public class CrearPersonalCommandHandler : IRequestHandler<CrearPersonalCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CrearPersonalCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CrearPersonalCommand request, CancellationToken cancellationToken)
    {
        _ = await _context.UnidadesRespuesta.FindAsync([request.UnidadRespuestaId], cancellationToken)
            ?? throw new NotFoundException(nameof(UnidadRespuesta), request.UnidadRespuestaId.ToString());

        var entity = new Domain.Entities.Personal
        {
            Nombres = request.Nombres,
            Apellidos = request.Apellidos,
            Documento = request.Documento,
            InstitucionId = request.InstitucionId,
            Especialidad = request.Especialidad,
            Funcion = request.Funcion,
            Certificaciones = request.Certificaciones,
            Disponible = request.Disponible,
            UnidadRespuestaId = request.UnidadRespuestaId
        };

        _context.Personal.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
