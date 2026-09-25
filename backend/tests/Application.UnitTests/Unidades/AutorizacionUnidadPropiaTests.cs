using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Application.Unidades.Commands.CambiarEstadoOperativoUnidad;
using Sige.Domain.Constants;
using Sige.Domain.Entities;
using Sige.Domain.Enums;
using Sige.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Sige.Application.UnitTests.Unidades;

// FR-120 (agregado tras /speckit-analyze, hallazgo C1): un usuario con el UNICO
// rol "UnidadDeRespuesta" solo puede cambiar el estado operativo de SU PROPIA
// unidad (UnidadRespuesta.UsuarioId == IUser.Id), no de cualquier otra.
public class AutorizacionUnidadPropiaTests
{
    private ApplicationDbContext _context = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    private UnidadRespuesta CrearUnidad(string? usuarioId)
    {
        var unidad = new UnidadRespuesta { Tipo = TipoUnidad.Ambulancia, Identificador = Guid.NewGuid().ToString(), UsuarioId = usuarioId };
        _context.UnidadesRespuesta.Add(unidad);
        _context.SaveChanges();
        return unidad;
    }

    private static CambiarEstadoOperativoUnidadCommandHandler CrearHandler(ApplicationDbContext context, string userId, params string[] roles)
    {
        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns(userId);
        user.Setup(u => u.Roles).Returns(roles.ToList());

        return new CambiarEstadoOperativoUnidadCommandHandler(context, user.Object);
    }

    [Test]
    public void DebeRechazar_CuandoUnidadDeRespuestaIntentaCambiarUnidadAjena()
    {
        var unidadDeOtro = CrearUnidad(usuarioId: "usuario-dueno");
        var handler = CrearHandler(_context, "usuario-intruso", Roles.UnidadDeRespuesta);

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await handler.Handle(
                new CambiarEstadoOperativoUnidadCommand { UnidadId = unidadDeOtro.Id, NuevoEstado = EstadoOperativoUnidad.Ocupada },
                CancellationToken.None));
    }

    [Test]
    public async Task DebePermitir_CuandoUnidadDeRespuestaCambiaSuPropiaUnidad()
    {
        var unidadPropia = CrearUnidad(usuarioId: "usuario-dueno");
        var handler = CrearHandler(_context, "usuario-dueno", Roles.UnidadDeRespuesta);

        await handler.Handle(
            new CambiarEstadoOperativoUnidadCommand { UnidadId = unidadPropia.Id, NuevoEstado = EstadoOperativoUnidad.Ocupada },
            CancellationToken.None);

        var actualizada = await _context.UnidadesRespuesta.FindAsync(unidadPropia.Id);
        Assert.That(actualizada!.EstadoOperativo, Is.EqualTo(EstadoOperativoUnidad.Ocupada));
    }

    [Test]
    public async Task DebePermitir_CuandoSupervisorCambiaCualquierUnidad()
    {
        // Operador/Supervisor no quedan sujetos a la restriccion de "unidad propia".
        var unidadDeOtro = CrearUnidad(usuarioId: "usuario-dueno");
        var handler = CrearHandler(_context, "sup-1", Roles.Supervisor);

        await handler.Handle(
            new CambiarEstadoOperativoUnidadCommand { UnidadId = unidadDeOtro.Id, NuevoEstado = EstadoOperativoUnidad.FueraDeServicio },
            CancellationToken.None);

        var actualizada = await _context.UnidadesRespuesta.FindAsync(unidadDeOtro.Id);
        Assert.That(actualizada!.EstadoOperativo, Is.EqualTo(EstadoOperativoUnidad.FueraDeServicio));
    }
}
