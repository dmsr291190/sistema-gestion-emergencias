using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Constants;
using Sige.Domain.Entities;
using Sige.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.Emergencias.Commands.ReabrirEmergencia;

// FR-017: reapertura restringida a Supervisor, sin limite de veces (cada una audita).
// Las unidades NO se reasignan automaticamente al reabrir.
[Authorize(Roles = Roles.Supervisor)]
public record ReabrirEmergenciaCommand : IRequest
{
    public required int EmergenciaId { get; init; }
}

public class ReabrirEmergenciaCommandHandler : IRequestHandler<ReabrirEmergenciaCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public ReabrirEmergenciaCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(ReabrirEmergenciaCommand request, CancellationToken cancellationToken)
    {
        var emergencia = await _context.Emergencias
            .FirstOrDefaultAsync(e => e.Id == request.EmergenciaId, cancellationToken)
            ?? throw new NotFoundException(nameof(Emergencia), request.EmergenciaId.ToString());

        if (emergencia.Estado != EstadoEmergencia.Cerrada)
        {
            throw new ConflictException("EMERGENCIA_NO_CERRADA", "Solo se puede reabrir una emergencia cerrada.");
        }

        emergencia.Estado = EstadoEmergencia.Atendida;

        _context.EventosAuditoria.Add(new EventoAuditoria
        {
            EmergenciaId = emergencia.Id,
            TipoEvento = "EmergenciaReabierta",
            EstadoAnterior = nameof(EstadoEmergencia.Cerrada),
            EstadoNuevo = nameof(EstadoEmergencia.Atendida),
            UsuarioId = _user.Id,
            FechaHora = DateTimeOffset.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
