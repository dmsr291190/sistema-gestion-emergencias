using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Constants;
using Sige.Domain.Enums;

namespace Sige.Application.TiposEmergencia.Commands.EditarTipoEmergencia;

// FR-101, FR-102: editar o activar/desactivar un tipo del catálogo. Desactivar no
// borra ni afecta las emergencias existentes que ya lo usan.
[Authorize(Roles = Roles.Administrador)]
public record EditarTipoEmergenciaCommand : IRequest
{
    public required int Id { get; init; }

    public required string Nombre { get; init; }

    public Ambito Ambito { get; init; }

    public string? Icono { get; init; }

    public string? Color { get; init; }

    public Prioridad PrioridadPorDefecto { get; init; }

    public bool Activo { get; init; }
}

public class EditarTipoEmergenciaCommandHandler : IRequestHandler<EditarTipoEmergenciaCommand>
{
    private readonly IApplicationDbContext _context;

    public EditarTipoEmergenciaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(EditarTipoEmergenciaCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TiposEmergencia.FindAsync([request.Id], cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.TipoEmergencia), request.Id.ToString());

        entity.Nombre = request.Nombre;
        entity.Ambito = request.Ambito;
        entity.Icono = request.Icono;
        entity.Color = request.Color;
        entity.PrioridadPorDefecto = request.PrioridadPorDefecto;
        entity.Activo = request.Activo;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
