using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Constants;
using Sige.Domain.Entities;
using Sige.Domain.Enums;

namespace Sige.Application.Unidades.Commands.CambiarEstadoOperativoUnidad;

// FR-004: cambio de estado operativo restringido al rol Supervisor.
// FR-120 (ampliacion 002): tambien lo puede hacer un usuario con rol
// "UnidadDeRespuesta", pero SOLO sobre su propia unidad -- ver la verificacion
// de propiedad en el Handler (no se puede expresar por rol en [Authorize]).
// Nota (hallazgo U1/CHK012 de Analyze, todavia diferido): si la unidad tiene una
// asignacion activa y se marca "FueraDeServicio", el sistema no reasigna
// automaticamente esa asignacion; queda como trabajo pendiente documentado.
[Authorize(Roles = Roles.Supervisor)]
[Authorize(Roles = Roles.UnidadDeRespuesta)]
public record CambiarEstadoOperativoUnidadCommand : IRequest
{
    public required int UnidadId { get; init; }

    public required EstadoOperativoUnidad NuevoEstado { get; init; }
}

public class CambiarEstadoOperativoUnidadCommandHandler : IRequestHandler<CambiarEstadoOperativoUnidadCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CambiarEstadoOperativoUnidadCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(CambiarEstadoOperativoUnidadCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.UnidadesRespuesta
            .FindAsync([request.UnidadId], cancellationToken)
            ?? throw new NotFoundException(nameof(UnidadRespuesta), request.UnidadId.ToString());

        // FR-120: si el UNICO rol del caller es "UnidadDeRespuesta" (no tiene
        // Supervisor), solo puede tocar su propia unidad.
        var esSoloUnidadDeRespuesta = (_user.Roles ?? []).Contains(Roles.UnidadDeRespuesta)
            && !(_user.Roles ?? []).Contains(Roles.Supervisor);

        if (esSoloUnidadDeRespuesta && entity.UsuarioId != _user.Id)
        {
            throw new ForbiddenAccessException();
        }

        var estadoAnterior = entity.EstadoOperativo;
        entity.EstadoOperativo = request.NuevoEstado;

        _context.EventosAuditoria.Add(new EventoAuditoria
        {
            UnidadId = entity.Id,
            TipoEvento = "CambioEstadoUnidad",
            EstadoAnterior = estadoAnterior.ToString(),
            EstadoNuevo = request.NuevoEstado.ToString(),
            UsuarioId = _user.Id,
            FechaHora = DateTimeOffset.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
