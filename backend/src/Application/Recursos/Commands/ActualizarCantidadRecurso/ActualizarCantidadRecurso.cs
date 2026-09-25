using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Security;
using Sige.Domain.Constants;

namespace Sige.Application.Recursos.Commands.ActualizarCantidadRecurso;

// FR-112: actualizar la cantidad disponible de un recurso ya registrado.
[Authorize(Roles = $"{Roles.CoordinadorLogistico},{Roles.JefeDeUnidad}")]
public record ActualizarCantidadRecursoCommand : IRequest
{
    public required int Id { get; init; }

    public required int CantidadDisponible { get; init; }
}

public class ActualizarCantidadRecursoCommandValidator : AbstractValidator<ActualizarCantidadRecursoCommand>
{
    public ActualizarCantidadRecursoCommandValidator()
    {
        RuleFor(v => v.CantidadDisponible).GreaterThanOrEqualTo(0).WithMessage("La cantidad disponible no puede ser negativa.");
    }
}

public class ActualizarCantidadRecursoCommandHandler : IRequestHandler<ActualizarCantidadRecursoCommand>
{
    private readonly IApplicationDbContext _context;

    public ActualizarCantidadRecursoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ActualizarCantidadRecursoCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Recursos.FindAsync([request.Id], cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Recurso), request.Id.ToString());

        // data-model.md: CantidadDisponible <= Cantidad (hallazgo U1 de Analyze).
        if (request.CantidadDisponible > entity.Cantidad)
        {
            throw new ConflictException("CANTIDAD_DISPONIBLE_INVALIDA", "La cantidad disponible no puede superar la cantidad total del recurso.");
        }

        entity.CantidadDisponible = request.CantidadDisponible;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
