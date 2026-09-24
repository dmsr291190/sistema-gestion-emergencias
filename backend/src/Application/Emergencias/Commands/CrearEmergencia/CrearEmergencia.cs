using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Entities;
using Sige.Domain.Enums;

namespace Sige.Application.Emergencias.Commands.CrearEmergencia;

// FR-001, FR-002, FR-015, FR-019
[Authorize]
public record CrearEmergenciaCommand : IRequest<int>
{
    public required string Tipo { get; init; }

    public required string Descripcion { get; init; }

    public double Latitud { get; init; }

    public double Longitud { get; init; }

    public Prioridad Prioridad { get; init; }

    public required string ReportanteNombre { get; init; }

    public string? ReportanteContacto { get; init; }
}

public class CrearEmergenciaCommandHandler : IRequestHandler<CrearEmergenciaCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CrearEmergenciaCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<int> Handle(CrearEmergenciaCommand request, CancellationToken cancellationToken)
    {
        var fechaHora = DateTimeOffset.UtcNow;

        var entity = new Emergencia
        {
            Tipo = request.Tipo,
            Descripcion = request.Descripcion,
            Latitud = request.Latitud,
            Longitud = request.Longitud,
            Prioridad = request.Prioridad,
            FechaHoraReporte = fechaHora,
            ReportanteNombre = request.ReportanteNombre,
            ReportanteContacto = request.ReportanteContacto,
            Estado = EstadoEmergencia.Reportada,
            CreadoPorId = _user.Id
        };

        entity.EventosAuditoria.Add(new EventoAuditoria
        {
            TipoEvento = "EmergenciaCreada",
            EstadoNuevo = nameof(EstadoEmergencia.Reportada),
            UsuarioId = _user.Id,
            FechaHora = fechaHora
        });

        _context.Emergencias.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
