using Microsoft.AspNetCore.Identity;
using Sige.Domain.Entities;

namespace Sige.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    // FR-118, FR-119: datos de administracion y trazabilidad de la ampliacion.
    // No "required": MapIdentityApi<TUser>() exige que TUser satisfaga new(), lo
    // cual es incompatible con miembros required (CS9040).
    public string NombreCompleto { get; set; } = string.Empty;

    public int? InstitucionId { get; set; }

    public Institucion? Institucion { get; set; }

    public DateTimeOffset? UltimoAcceso { get; set; }

    public bool RequiereCambioPassword { get; set; }

    public string? CreadoPorId { get; set; }

    public string? ModificadoPorId { get; set; }
}
