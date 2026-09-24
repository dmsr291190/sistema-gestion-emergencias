using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Constants;
using Sige.Domain.Entities;
using Sige.Domain.Enums;

namespace Sige.Application.Unidades.Commands.CambiarEstadoOperativoUnidad;

// FR-004: cambio de estado operativo restringido al rol Supervisor.
// Nota (hallazgo U1/CHK012 de Analyze, todavia diferido): si la unidad tiene una
// asignacion activa y se marca "FueraDeServicio", el sistema no reasigna
// automaticamente esa asignacion; queda como trabajo pendiente documentado.
[Authorize(Roles = Roles.Supervisor)]
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
            ?? throw new KeyNotFoundException($"Unidad {request.UnidadId} no encontrada.");

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
