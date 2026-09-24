using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Entities;
using Sige.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.Asignaciones.Commands.CambiarEstadoAsignacion;

// FR-016: progreso independiente por unidad; el estado general de la emergencia se
// deriva del conjunto de asignaciones (una sola -> sigue su avance; varias -> pasa a
// "Atendida" unicamente cuando todas llegan a "Atendida").
[Authorize]
public record CambiarEstadoAsignacionCommand : IRequest
{
    public required int AsignacionId { get; init; }

    public required EstadoAsignacion NuevoEstado { get; init; }
}

public class CambiarEstadoAsignacionCommandHandler : IRequestHandler<CambiarEstadoAsignacionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CambiarEstadoAsignacionCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(CambiarEstadoAsignacionCommand request, CancellationToken cancellationToken)
    {
        var asignacion = await _context.Asignaciones
            .FirstOrDefaultAsync(a => a.Id == request.AsignacionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Asignacion), request.AsignacionId.ToString());

        if (request.NuevoEstado <= asignacion.EstadoAsignacion)
        {
            throw new ConflictException(
                "TRANSICION_INVALIDA",
                $"No se puede pasar de '{asignacion.EstadoAsignacion}' a '{request.NuevoEstado}'; solo se admite avanzar.");
        }

        var estadoAnterior = asignacion.EstadoAsignacion;
        asignacion.EstadoAsignacion = request.NuevoEstado;

        var fechaHora = DateTimeOffset.UtcNow;
        _context.EventosAuditoria.Add(new EventoAuditoria
        {
            EmergenciaId = asignacion.EmergenciaId,
            UnidadId = asignacion.UnidadId,
            TipoEvento = "CambioEstado",
            EstadoAnterior = estadoAnterior.ToString(),
            EstadoNuevo = request.NuevoEstado.ToString(),
            UsuarioId = _user.Id,
            FechaHora = fechaHora
        });

        await RecalcularEstadoEmergenciaAsync(asignacion.EmergenciaId, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task RecalcularEstadoEmergenciaAsync(int emergenciaId, CancellationToken cancellationToken)
    {
        var emergencia = await _context.Emergencias.FirstAsync(e => e.Id == emergenciaId, cancellationToken);

        // IMPORTANTE: se consultan las entidades completas (no una proyeccion escalar
        // con Select) para que EF Core use resolucion de identidad y devuelva, para la
        // asignacion que ya se modifico en este mismo Handle (todavia sin SaveChanges),
        // su valor en memoria actualizado en vez del valor todavia persistido en la BD.
        var asignaciones = await _context.Asignaciones
            .Where(a => a.EmergenciaId == emergenciaId)
            .ToListAsync(cancellationToken);
        var estadosAsignaciones = asignaciones.Select(a => a.EstadoAsignacion).ToList();

        if (estadosAsignaciones.Count == 1)
        {
            // FR-016 / data-model.md: con una sola unidad, la emergencia sigue su avance.
            emergencia.Estado = Enum.Parse<EstadoEmergencia>(estadosAsignaciones[0].ToString());
        }
        else if (estadosAsignaciones.Count > 1 && estadosAsignaciones.All(e => e == EstadoAsignacion.Atendida))
        {
            emergencia.Estado = EstadoEmergencia.Atendida;
        }
    }
}
