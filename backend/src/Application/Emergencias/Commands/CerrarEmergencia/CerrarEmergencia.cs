using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Constants;
using Sige.Domain.Entities;
using Sige.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.Emergencias.Commands.CerrarEmergencia;

// FR-017, FR-018: cierre restringido a Supervisor; libera las unidades asignadas
// (salvo que ya esten fuera de servicio por un motivo independiente).
[Authorize(Roles = Roles.Supervisor)]
public record CerrarEmergenciaCommand : IRequest
{
    public required int EmergenciaId { get; init; }
}

public class CerrarEmergenciaCommandHandler : IRequestHandler<CerrarEmergenciaCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CerrarEmergenciaCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(CerrarEmergenciaCommand request, CancellationToken cancellationToken)
    {
        var emergencia = await _context.Emergencias
            .FirstOrDefaultAsync(e => e.Id == request.EmergenciaId, cancellationToken)
            ?? throw new NotFoundException(nameof(Emergencia), request.EmergenciaId.ToString());

        if (emergencia.Estado != EstadoEmergencia.Atendida)
        {
            throw new ConflictException(
                "EMERGENCIA_NO_ATENDIDA",
                "Solo se puede cerrar una emergencia cuyas unidades ya esten todas 'Atendida'.");
        }

        var unidadIds = await _context.Asignaciones
            .Where(a => a.EmergenciaId == emergencia.Id)
            .Select(a => a.UnidadId)
            .ToListAsync(cancellationToken);

        // FR-018: liberar unidades salvo que ya esten fuera de servicio por otro motivo.
        await _context.UnidadesRespuesta
            .Where(u => unidadIds.Contains(u.Id) && u.EstadoOperativo != EstadoOperativoUnidad.FueraDeServicio)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.EstadoOperativo, EstadoOperativoUnidad.Disponible), cancellationToken);

        emergencia.Estado = EstadoEmergencia.Cerrada;

        _context.EventosAuditoria.Add(new EventoAuditoria
        {
            EmergenciaId = emergencia.Id,
            TipoEvento = "EmergenciaCerrada",
            EstadoAnterior = nameof(EstadoEmergencia.Atendida),
            EstadoNuevo = nameof(EstadoEmergencia.Cerrada),
            UsuarioId = _user.Id,
            FechaHora = DateTimeOffset.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
