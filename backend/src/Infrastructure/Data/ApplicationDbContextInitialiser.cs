using Sige.Domain.Constants;
using Sige.Domain.Entities;
using Sige.Domain.Enums;
using Sige.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sige.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Roles del MVP (FR-012).
        foreach (var roleName in new[] { Roles.Operador, Roles.Supervisor })
        {
            if (_roleManager.Roles.All(r => r.Name != roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Usuarios demo (T015): 1 Operador, 1 Supervisor.
        await CreateDemoUserIfMissingAsync("operador@sige.local", "Operador123!", Roles.Operador);
        await CreateDemoUserIfMissingAsync("supervisor@sige.local", "Supervisor123!", Roles.Supervisor);

        // Unidades demo (T015): ambulancia, bomberos y patrullero, todas disponibles.
        if (!_context.UnidadesRespuesta.Any())
        {
            _context.UnidadesRespuesta.AddRange(
                new UnidadRespuesta
                {
                    Tipo = TipoUnidad.Ambulancia,
                    Identificador = "AMB-01",
                    EstadoOperativo = EstadoOperativoUnidad.Disponible
                },
                new UnidadRespuesta
                {
                    Tipo = TipoUnidad.Bomberos,
                    Identificador = "BOM-01",
                    EstadoOperativo = EstadoOperativoUnidad.Disponible
                },
                new UnidadRespuesta
                {
                    Tipo = TipoUnidad.Patrullero,
                    Identificador = "PAT-01",
                    EstadoOperativo = EstadoOperativoUnidad.Disponible
                });

            await _context.SaveChangesAsync();
        }
    }

    private async Task CreateDemoUserIfMissingAsync(string email, string password, string role)
    {
        if (_userManager.Users.All(u => u.UserName != email))
        {
            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
