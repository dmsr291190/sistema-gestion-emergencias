using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Entities;
using Sige.Domain.Enums;

namespace Sige.Application.Emergencias.Commands.ValidarEmergencia;

// FR-009: transicion explicita "reportada" -> "validada", requisito previo a asignar
// unidades (FR-006). Agregado tras /speckit-analyze (hallazgo C1).
[Authorize]
public record ValidarEmergenciaCommand : IRequest
{
    public required int EmergenciaId { get; init; }
}

public class ValidarEmergenciaCommandHandler : IRequestHandler<ValidarEmergenciaCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public ValidarEmergenciaCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(ValidarEmergenciaCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Emergencias
            .FindAsync([request.EmergenciaId], cancellationToken)
            ?? throw new KeyNotFoundException($"Emergencia {request.EmergenciaId} no encontrada.");

        if (entity.Estado != EstadoEmergencia.Reportada)
        {
            throw new InvalidOperationException(
                $"Solo se puede validar una emergencia en estado 'Reportada' (actual: '{entity.Estado}').");
        }

        var estadoAnterior = entity.Estado;
        entity.Estado = EstadoEmergencia.Validada;

        var fechaHora = DateTimeOffset.UtcNow;
        entity.EventosAuditoria.Add(new EventoAuditoria
        {
            TipoEvento = "EmergenciaValidada",
            EstadoAnterior = estadoAnterior.ToString(),
            EstadoNuevo = nameof(EstadoEmergencia.Validada),
            UsuarioId = _user.Id,
            FechaHora = fechaHora
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
