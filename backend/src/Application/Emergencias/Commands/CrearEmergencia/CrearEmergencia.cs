using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Entities;
using Sige.Domain.Enums;
using Sige.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.Emergencias.Commands.CrearEmergencia;

// FR-104: forma de entrada de la ubicacion; se traduce a Domain.ValueObjects.Ubicacion.
public record UbicacionInput
{
    public string? Departamento { get; init; }
    public string? Provincia { get; init; }
    public string? Distrito { get; init; }
    public string? CentroPoblado { get; init; }
    public string? Direccion { get; init; }
    public string? Referencia { get; init; }
    public double Latitud { get; init; }
    public double Longitud { get; init; }
    public Ambito Ambito { get; init; }
    public bool SinDireccionFormal { get; init; }
}

// FR-001, FR-002, FR-015, FR-019, FR-104, FR-105, FR-125 (ampliacion 002)
[Authorize]
public record CrearEmergenciaCommand : IRequest<int>
{
    public required int TipoEmergenciaId { get; init; }

    public required string Descripcion { get; init; }

    public required UbicacionInput Ubicacion { get; init; }

    public int Afectados { get; init; }

    public int Heridos { get; init; }

    public int Desaparecidos { get; init; }

    public int Fallecidos { get; init; }

    public int Evacuados { get; init; }

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
        // FR-102: un tipo desactivado no puede usarse para emergencias nuevas.
        var tipo = await _context.TiposEmergencia
            .FirstOrDefaultAsync(t => t.Id == request.TipoEmergenciaId, cancellationToken)
            ?? throw new NotFoundException(nameof(TipoEmergencia), request.TipoEmergenciaId.ToString());

        if (!tipo.Activo)
        {
            throw new ConflictException("TIPO_EMERGENCIA_INACTIVO", "El tipo de emergencia seleccionado esta desactivado.");
        }

        var fechaHora = DateTimeOffset.UtcNow;

        var entity = new Emergencia
        {
            TipoEmergenciaId = tipo.Id,
            Tipo = tipo.Nombre,
            Descripcion = request.Descripcion,
            Ubicacion = new Ubicacion
            {
                Departamento = request.Ubicacion.Departamento,
                Provincia = request.Ubicacion.Provincia,
                Distrito = request.Ubicacion.Distrito,
                CentroPoblado = request.Ubicacion.CentroPoblado,
                Direccion = request.Ubicacion.Direccion,
                Referencia = request.Ubicacion.Referencia,
                Latitud = request.Ubicacion.Latitud,
                Longitud = request.Ubicacion.Longitud,
                Ambito = request.Ubicacion.Ambito,
                SinDireccionFormal = request.Ubicacion.SinDireccionFormal
            },
            Afectados = request.Afectados,
            Heridos = request.Heridos,
            Desaparecidos = request.Desaparecidos,
            Fallecidos = request.Fallecidos,
            Evacuados = request.Evacuados,
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
