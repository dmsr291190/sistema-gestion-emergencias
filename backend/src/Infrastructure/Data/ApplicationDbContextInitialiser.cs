using Sige.Domain.Constants;
using Sige.Domain.Entities;
using Sige.Domain.Enums;
using Sige.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    // FR-122, FR-124: los usuarios y datos demo solo se generan en estos entornos;
    // nunca en Production. "Development" es el que usa `dotnet run` localmente.
    private static readonly string[] EntornosPermitidosParaDemo = ["Development", "Demo", "Testing"];

    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IHostEnvironment _environment;

    public ApplicationDbContextInitialiser(
        ILogger<ApplicationDbContextInitialiser> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IHostEnvironment environment)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _environment = environment;
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
        // FR-117: todos los roles (MVP + ampliacion) se siembran siempre, en
        // cualquier entorno -- no son datos demo, son parte del esquema de
        // autorizacion.
        foreach (var roleName in Roles.Todos)
        {
            if (_roleManager.Roles.All(r => r.Name != roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Catalogo inicial de instituciones (agregado tras Analyze, C2) -- no es
        // un dato demo, es un catalogo base que Personal/UnidadRespuesta/Usuario
        // necesitan poder referenciar en cualquier entorno.
        await SeedInstitucionesAsync();

        // FR-103: migra las emergencias del MVP (Tipo texto libre) al catalogo de
        // TipoEmergencia -- corre siempre, no es un dato demo, es una migracion de
        // datos existentes.
        await MigrarTiposLegadosAsync();

        // FR-122, FR-124: a partir de aqui, todo lo que sigue es demo -- nunca en
        // Production ni en cualquier entorno no listado explicitamente.
        if (!EntornosPermitidosParaDemo.Contains(_environment.EnvironmentName))
        {
            _logger.LogInformation(
                "Entorno '{Entorno}' no esta en la lista de entornos permitidos para datos demo; se omite el seed.",
                _environment.EnvironmentName);
            return;
        }

        await SeedUsuariosDemoAsync();
        await SeedDatosDemoAsync();
    }

    private async Task SeedInstitucionesAsync()
    {
        if (_context.Instituciones.Any())
        {
            return;
        }

        string[] nombres =
        [
            "Bomberos", "Policía Nacional", "Ministerio de Salud", "Marina de Guerra",
            "Fuerza Aérea", "Defensa Civil (INDECI)", "Cruz Roja"
        ];

        _context.Instituciones.AddRange(nombres.Select(n => new Institucion { Nombre = n, Activo = true }));

        await _context.SaveChangesAsync();
    }

    private async Task MigrarTiposLegadosAsync()
    {
        var emergenciasSinCatalogo = await _context.Emergencias
            .Where(e => e.TipoEmergenciaId == null && e.Tipo != null)
            .ToListAsync();

        if (emergenciasSinCatalogo.Count == 0)
        {
            return;
        }

        foreach (var emergencia in emergenciasSinCatalogo)
        {
            var nombreTipo = emergencia.Tipo!;

            var tipo = await _context.TiposEmergencia
                .FirstOrDefaultAsync(t => t.Nombre.ToLower() == nombreTipo.ToLower());

            if (tipo == null)
            {
                // FR-103: el tipo creado por la migracion queda activo por
                // defecto (Clarification confirmada por Diego), para no
                // bloquear el registro de nuevas emergencias de ese tipo.
                tipo = new TipoEmergencia
                {
                    Nombre = nombreTipo,
                    Ambito = Ambito.Terrestre,
                    PrioridadPorDefecto = Prioridad.Media,
                    Icono = "cilWarning",
                    Color = "#6c757d",
                    Activo = true
                };

                _context.TiposEmergencia.Add(tipo);
                await _context.SaveChangesAsync();
            }

            emergencia.TipoEmergenciaId = tipo.Id;
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedUsuariosDemoAsync()
    {
        var demoPassword = Environment.GetEnvironmentVariable("SIGE_DEMO_PASSWORD") ?? "DemoSige#2026";

        // Usuarios del MVP (T015 original) -- se conservan con su correo historico.
        await CrearUsuarioDemoSiFaltaAsync("operador@sige.local", "Operador Demo", demoPassword, Roles.Operador);
        await CrearUsuarioDemoSiFaltaAsync("supervisor@sige.local", "Supervisor Demo", demoPassword, Roles.Supervisor);

        // FR-122, sección 19.1 del documento de origen: 10 cuentas demo, una por rol nuevo.
        await CrearUsuarioDemoSiFaltaAsync("admin.sige", "Administrador Demo", demoPassword, Roles.Administrador);
        await CrearUsuarioDemoSiFaltaAsync("supervisor.sige", "Supervisor Nacional Demo", demoPassword, Roles.Supervisor);
        await CrearUsuarioDemoSiFaltaAsync("operador.lima", "Operador Lima Demo", demoPassword, Roles.Operador);
        await CrearUsuarioDemoSiFaltaAsync("operador.norte", "Operador Norte Demo", demoPassword, Roles.Operador);
        await CrearUsuarioDemoSiFaltaAsync("logistica.sige", "Coordinador Logístico Demo", demoPassword, Roles.CoordinadorLogistico);
        await CrearUsuarioDemoSiFaltaAsync("jefe.unidad01", "Jefe de Unidad Demo", demoPassword, Roles.JefeDeUnidad);
        await CrearUsuarioDemoSiFaltaAsync("unidad.maritima01", "Unidad Marítima Demo", demoPassword, Roles.UnidadDeRespuesta);
        await CrearUsuarioDemoSiFaltaAsync("unidad.terrestre01", "Unidad Terrestre Demo", demoPassword, Roles.UnidadDeRespuesta);
        await CrearUsuarioDemoSiFaltaAsync("visor.sige", "Visualizador Demo", demoPassword, Roles.Visualizador);
        await CrearUsuarioDemoSiFaltaAsync("prueba.restringida", "Usuario Restringido Demo", demoPassword, Roles.Visualizador);
    }

    private async Task<ApplicationUser?> CrearUsuarioDemoSiFaltaAsync(string userName, string nombreCompleto, string password, string role)
    {
        var existente = _userManager.Users.FirstOrDefault(u => u.UserName == userName);

        if (existente != null)
        {
            return existente;
        }

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName.Contains('@') ? userName : $"{userName}@sige.local",
            EmailConfirmed = true,
            NombreCompleto = nombreCompleto
        };

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, role);
            return user;
        }

        _logger.LogWarning("No se pudo crear el usuario demo {UserName}: {Errores}", userName,
            string.Join("; ", result.Errors.Select(e => e.Description)));

        return null;
    }

    private async Task SeedDatosDemoAsync()
    {
        // Unidades demo del MVP (T015 original): ambulancia, bomberos y patrullero.
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

        // FR-122: volumen de datos demo ampliado -- ver DemoDataSeeder (T028, US2).
        await DemoDataSeeder.SeedAsync(_context, _userManager);
    }
}
