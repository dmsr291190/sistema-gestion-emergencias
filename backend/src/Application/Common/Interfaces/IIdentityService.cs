using Sige.Application.Common.Models;

namespace Sige.Application.Common.Interfaces;

public class UsuarioAdminDto
{
    public required string Id { get; init; }
    public required string UserName { get; init; }
    public string? Email { get; init; }
    public required string NombreCompleto { get; init; }
    public int? InstitucionId { get; init; }
    public required List<string> Roles { get; init; }
    public bool Bloqueado { get; init; }
    public bool RequiereCambioPassword { get; init; }
    public DateTimeOffset? UltimoAcceso { get; init; }
    public int IntentosFallidos { get; init; }
    public string? CreadoPorId { get; init; }
    public string? ModificadoPorId { get; init; }
}

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(string userId);

    // FR-118, FR-119: administracion de usuarios y roles (US1, ampliacion 002).
    Task<List<UsuarioAdminDto>> ListarUsuariosAsync();

    Task<(Result Result, string UserId)> CrearUsuarioAsync(
        string userName, string nombreCompleto, string password, int? institucionId, IEnumerable<string> roles, string? creadoPorId);

    Task<Result> EditarUsuarioAsync(string userId, string nombreCompleto, int? institucionId, IEnumerable<string> roles, string? modificadoPorId);

    Task<Result> BloquearUsuarioAsync(string userId);

    Task<Result> DesbloquearUsuarioAsync(string userId);

    Task<Result> ForzarCambioPasswordAsync(string userId, string passwordTemporal);

    Task<Result> CambiarMiPasswordAsync(string userId, string passwordActual, string passwordNueva);

    Task RegistrarUltimoAccesoAsync(string userId);
}
