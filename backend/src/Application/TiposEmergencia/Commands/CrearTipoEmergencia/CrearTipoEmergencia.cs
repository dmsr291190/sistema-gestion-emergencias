using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Constants;
using Sige.Domain.Entities;
using Sige.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Sige.Application.TiposEmergencia.Commands.CrearTipoEmergencia;

// FR-101: solo el rol Administrador puede crear tipos del catálogo.
[Authorize(Roles = Roles.Administrador)]
public record CrearTipoEmergenciaCommand : IRequest<int>
{
    public required string Nombre { get; init; }

    public Ambito Ambito { get; init; }

    public string? Icono { get; init; }

    public string? Color { get; init; }

    public Prioridad PrioridadPorDefecto { get; init; }
}

public class CrearTipoEmergenciaCommandHandler : IRequestHandler<CrearTipoEmergenciaCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CrearTipoEmergenciaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CrearTipoEmergenciaCommand request, CancellationToken cancellationToken)
    {
        var existe = await _context.TiposEmergencia
            .AnyAsync(t => t.Nombre.ToLower() == request.Nombre.ToLower(), cancellationToken);

        if (existe)
        {
            throw new ConflictException("TIPO_EMERGENCIA_DUPLICADO", "Ya existe un tipo de emergencia con ese nombre.");
        }

        var entity = new Domain.Entities.TipoEmergencia
        {
            Nombre = request.Nombre,
            Ambito = request.Ambito,
            Icono = request.Icono,
            Color = request.Color,
            PrioridadPorDefecto = request.PrioridadPorDefecto,
            Activo = true
        };

        _context.TiposEmergencia.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
