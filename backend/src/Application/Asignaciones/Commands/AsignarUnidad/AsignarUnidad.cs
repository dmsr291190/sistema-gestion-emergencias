using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Entities;
using Sige.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.Asignaciones.Commands.AsignarUnidad;

// FR-006, FR-007, FR-009, FR-010. Decision de Clarify (sesion 2026-09-26): control
// optimista, revalidando disponibilidad en el momento de confirmar.
[Authorize]
public record AsignarUnidadCommand : IRequest<int>
{
    public required int EmergenciaId { get; init; }

    public required int UnidadId { get; init; }
}

public class AsignarUnidadCommandHandler : IRequestHandler<AsignarUnidadCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public AsignarUnidadCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<int> Handle(AsignarUnidadCommand request, CancellationToken cancellationToken)
    {
        var emergencia = await _context.Emergencias
            .FindAsync([request.EmergenciaId], cancellationToken)
            ?? throw new NotFoundException(nameof(Emergencia), request.EmergenciaId.ToString());

        if (emergencia.Estado == EstadoEmergencia.Reportada)
        {
            throw new ConflictException(
                "EMERGENCIA_NO_VALIDADA",
                "La emergencia debe estar 'Validada' antes de poder asignarle unidades (FR-009).");
        }

        if (emergencia.Estado == EstadoEmergencia.Cerrada)
        {
            throw new ConflictException(
                "EMERGENCIA_CERRADA",
                "No se pueden asignar unidades a una emergencia cerrada.");
        }

        // Revalidacion optimista atomica (FR-007): un UPDATE condicional en una sola
        // instruccion SQL evita la carrera de dos operadores asignando la misma unidad
        // casi al mismo tiempo, sin necesitar una columna de concurrencia dedicada
        // (ver research.md #4 y data-model.md). No se envuelve junto con el resto de
        // esta operacion en una unica transaccion explicita: es una simplificacion
        // aceptada para el alcance del MVP (ver bitacora).
        var unidadesActualizadas = await _context.UnidadesRespuesta
            .Where(u => u.Id == request.UnidadId && u.EstadoOperativo == EstadoOperativoUnidad.Disponible)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.EstadoOperativo, EstadoOperativoUnidad.Ocupada), cancellationToken);

        if (unidadesActualizadas == 0)
        {
            throw new ConflictException(
                "UNIDAD_NO_DISPONIBLE",
                "La unidad ya no esta disponible; puede haber sido asignada por otro operador.");
        }

        var fechaHora = DateTimeOffset.UtcNow;

        _context.Asignaciones.Add(new Asignacion
        {
            EmergenciaId = emergencia.Id,
            UnidadId = request.UnidadId,
            EstadoAsignacion = EstadoAsignacion.Despachada,
            AsignadoPorId = _user.Id,
            FechaHoraAsignacion = fechaHora
        });

        if (emergencia.Estado == EstadoEmergencia.Validada)
        {
            emergencia.Estado = EstadoEmergencia.Despachada;
        }

        _context.EventosAuditoria.Add(new EventoAuditoria
        {
            EmergenciaId = emergencia.Id,
            UnidadId = request.UnidadId,
            TipoEvento = "UnidadAsignada",
            EstadoNuevo = nameof(EstadoAsignacion.Despachada),
            UsuarioId = _user.Id,
            FechaHora = fechaHora
        });

        await _context.SaveChangesAsync(cancellationToken);

        return emergencia.Id;
    }
}
