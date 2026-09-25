using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Constants;

namespace Sige.Application.PersonalUnidades.Commands.EditarPersonal;

// FR-110: editar datos de personal ya registrado (incluida disponibilidad).
[Authorize(Roles = $"{Roles.CoordinadorLogistico},{Roles.JefeDeUnidad}")]
public record EditarPersonalCommand : IRequest
{
    public required int Id { get; init; }

    public required string Nombres { get; init; }

    public required string Apellidos { get; init; }

    public required string Documento { get; init; }

    public int? InstitucionId { get; init; }

    public string? Especialidad { get; init; }

    public string? Funcion { get; init; }

    public string? Certificaciones { get; init; }

    public bool Disponible { get; init; }
}

public class EditarPersonalCommandHandler : IRequestHandler<EditarPersonalCommand>
{
    private readonly IApplicationDbContext _context;

    public EditarPersonalCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(EditarPersonalCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Personal.FindAsync([request.Id], cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Personal), request.Id.ToString());

        entity.Nombres = request.Nombres;
        entity.Apellidos = request.Apellidos;
        entity.Documento = request.Documento;
        entity.InstitucionId = request.InstitucionId;
        entity.Especialidad = request.Especialidad;
        entity.Funcion = request.Funcion;
        entity.Certificaciones = request.Certificaciones;
        entity.Disponible = request.Disponible;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
