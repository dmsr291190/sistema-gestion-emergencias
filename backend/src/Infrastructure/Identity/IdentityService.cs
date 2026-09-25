using Sige.Application.Common.Interfaces;
using Sige.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Sige.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName,
            NombreCompleto = userName,
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }

    // FR-118, FR-119: administracion de usuarios y roles (US1, ampliacion 002).
    public async Task<List<UsuarioAdminDto>> ListarUsuariosAsync()
    {
        var usuarios = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync();
        var resultado = new List<UsuarioAdminDto>(usuarios.Count);

        foreach (var usuario in usuarios)
        {
            var roles = await _userManager.GetRolesAsync(usuario);
            var bloqueado = await _userManager.IsLockedOutAsync(usuario);

            resultado.Add(new UsuarioAdminDto
            {
                Id = usuario.Id,
                UserName = usuario.UserName ?? usuario.Id,
                Email = usuario.Email,
                NombreCompleto = usuario.NombreCompleto,
                InstitucionId = usuario.InstitucionId,
                Roles = roles.ToList(),
                Bloqueado = bloqueado,
                RequiereCambioPassword = usuario.RequiereCambioPassword,
                UltimoAcceso = usuario.UltimoAcceso,
                IntentosFallidos = usuario.AccessFailedCount,
                CreadoPorId = usuario.CreadoPorId,
                ModificadoPorId = usuario.ModificadoPorId
            });
        }

        return resultado;
    }

    public async Task<(Result Result, string UserId)> CrearUsuarioAsync(
        string userName, string nombreCompleto, string password, int? institucionId, IEnumerable<string> roles, string? creadoPorId)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName.Contains('@') ? userName : $"{userName}@sige.local",
            EmailConfirmed = true,
            NombreCompleto = nombreCompleto,
            InstitucionId = institucionId,
            CreadoPorId = creadoPorId,
            ModificadoPorId = creadoPorId
        };

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            var rolesList = roles.ToList();

            if (rolesList.Count > 0)
            {
                await _userManager.AddToRolesAsync(user, rolesList);
            }
        }

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<Result> EditarUsuarioAsync(string userId, string nombreCompleto, int? institucionId, IEnumerable<string> roles, string? modificadoPorId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Result.Failure(["Usuario no encontrado."]);
        }

        user.NombreCompleto = nombreCompleto;
        user.InstitucionId = institucionId;
        user.ModificadoPorId = modificadoPorId;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return updateResult.ToApplicationResult();
        }

        var rolesActuales = await _userManager.GetRolesAsync(user);
        var rolesNuevos = roles.ToList();

        var aQuitar = rolesActuales.Except(rolesNuevos).ToList();
        var aAgregar = rolesNuevos.Except(rolesActuales).ToList();

        if (aQuitar.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, aQuitar);
        }

        if (aAgregar.Count > 0)
        {
            await _userManager.AddToRolesAsync(user, aAgregar);
        }

        return Result.Success();
    }

    public async Task<Result> BloquearUsuarioAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Result.Failure(["Usuario no encontrado."]);
        }

        // FR-118: bloqueo indefinido hasta que un Administrador lo desbloquee.
        if (!user.LockoutEnabled)
        {
            await _userManager.SetLockoutEnabledAsync(user, true);
        }

        var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

        return result.ToApplicationResult();
    }

    public async Task<Result> DesbloquearUsuarioAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Result.Failure(["Usuario no encontrado."]);
        }

        var result = await _userManager.SetLockoutEndDateAsync(user, null);

        if (result.Succeeded)
        {
            await _userManager.ResetAccessFailedCountAsync(user);
        }

        return result.ToApplicationResult();
    }

    public async Task<Result> ForzarCambioPasswordAsync(string userId, string passwordTemporal)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Result.Failure(["Usuario no encontrado."]);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await _userManager.ResetPasswordAsync(user, token, passwordTemporal);

        if (!resetResult.Succeeded)
        {
            return resetResult.ToApplicationResult();
        }

        user.RequiereCambioPassword = true;
        await _userManager.UpdateAsync(user);

        return Result.Success();
    }

    public async Task<Result> CambiarMiPasswordAsync(string userId, string passwordActual, string passwordNueva)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Result.Failure(["Usuario no encontrado."]);
        }

        var result = await _userManager.ChangePasswordAsync(user, passwordActual, passwordNueva);

        if (!result.Succeeded)
        {
            return result.ToApplicationResult();
        }

        user.RequiereCambioPassword = false;
        await _userManager.UpdateAsync(user);

        return Result.Success();
    }

    public async Task RegistrarUltimoAccesoAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return;
        }

        user.UltimoAcceso = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);
    }
}
