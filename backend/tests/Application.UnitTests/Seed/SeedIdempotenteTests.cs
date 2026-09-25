using Sige.Infrastructure.Data;
using Sige.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Sige.Application.UnitTests.Seed;

// FR-123, SC-103: ejecutar el seed dos veces no debe duplicar usuarios, roles ni
// datos demo ya creados.
public class SeedIdempotenteTests
{
    private ApplicationDbContext _context = null!;
    private UserManager<ApplicationUser> _userManager = null!;
    private RoleManager<IdentityRole> _roleManager = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var userStore = new UserStore<ApplicationUser, IdentityRole, ApplicationDbContext>(_context);
        _userManager = new UserManager<ApplicationUser>(
            userStore,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            [new UserValidator<ApplicationUser>()],
            [new PasswordValidator<ApplicationUser>()],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            NullLogger<UserManager<ApplicationUser>>.Instance);

        var roleStore = new RoleStore<IdentityRole, ApplicationDbContext>(_context);
        _roleManager = new RoleManager<IdentityRole>(
            roleStore,
            [new RoleValidator<IdentityRole>()],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            NullLogger<RoleManager<IdentityRole>>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        _userManager.Dispose();
        _roleManager.Dispose();
        _context.Dispose();
    }

    private ApplicationDbContextInitialiser CrearInitialiser(string entorno)
    {
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns(entorno);

        return new ApplicationDbContextInitialiser(
            NullLogger<ApplicationDbContextInitialiser>.Instance, _context, _userManager, _roleManager, environment.Object);
    }

    [Test]
    public async Task TrySeedAsync_NoDebeDuplicar_AlEjecutarseDosVeces()
    {
        var initialiser = CrearInitialiser("Testing");

        await initialiser.TrySeedAsync();

        var rolesTrasPrimeraCorrida = await _context.Roles.CountAsync();
        var usuariosTrasPrimeraCorrida = await _userManager.Users.CountAsync();
        var institucionesTrasPrimeraCorrida = await _context.Instituciones.CountAsync();
        var unidadesTrasPrimeraCorrida = await _context.UnidadesRespuesta.CountAsync();
        var personalTrasPrimeraCorrida = await _context.Personal.CountAsync();

        Assert.That(usuariosTrasPrimeraCorrida, Is.GreaterThan(0), "El primer arranque debe crear usuarios demo.");
        Assert.That(unidadesTrasPrimeraCorrida, Is.GreaterThan(0), "El primer arranque debe crear unidades demo.");

        await initialiser.TrySeedAsync();

        Assert.That(await _context.Roles.CountAsync(), Is.EqualTo(rolesTrasPrimeraCorrida), "Los roles no deben duplicarse.");
        Assert.That(await _userManager.Users.CountAsync(), Is.EqualTo(usuariosTrasPrimeraCorrida), "Los usuarios demo no deben duplicarse.");
        Assert.That(await _context.Instituciones.CountAsync(), Is.EqualTo(institucionesTrasPrimeraCorrida), "El catálogo de instituciones no debe duplicarse.");
        Assert.That(await _context.UnidadesRespuesta.CountAsync(), Is.EqualTo(unidadesTrasPrimeraCorrida), "Las unidades demo no deben duplicarse.");
        Assert.That(await _context.Personal.CountAsync(), Is.EqualTo(personalTrasPrimeraCorrida), "El personal demo no debe duplicarse.");
    }

    [Test]
    public async Task TrySeedAsync_NoDebeCrearDatosDemo_EnEntornoNoPermitido()
    {
        var initialiser = CrearInitialiser("Production");

        await initialiser.TrySeedAsync();

        // FR-124: en Production no se generan usuarios ni datos demo.
        Assert.That(await _userManager.Users.CountAsync(), Is.EqualTo(0));
        Assert.That(await _context.UnidadesRespuesta.CountAsync(), Is.EqualTo(0));

        // Los roles y el catalogo de instituciones SI se siembran siempre (no son "demo").
        Assert.That(await _context.Roles.CountAsync(), Is.GreaterThan(0));
        Assert.That(await _context.Instituciones.CountAsync(), Is.GreaterThan(0));
    }
}
