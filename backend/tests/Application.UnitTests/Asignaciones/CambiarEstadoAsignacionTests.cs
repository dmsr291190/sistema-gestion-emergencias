using Sige.Application.Asignaciones.Commands.CambiarEstadoAsignacion;
using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Domain.Entities;
using Sige.Domain.Enums;
using Sige.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Sige.Application.UnitTests.Asignaciones;

// FR-016: el estado general de la emergencia se deriva del conjunto de asignaciones.
public class CambiarEstadoAsignacionTests
{
    private ApplicationDbContext _context = null!;
    private CambiarEstadoAsignacionCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns("sup-1");

        _handler = new CambiarEstadoAsignacionCommandHandler(_context, user.Object);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    private Emergencia CrearEmergenciaConAsignaciones(params EstadoAsignacion[] estados)
    {
        var emergencia = new Emergencia
        {
            Tipo = "Medica",
            Descripcion = "Test",
            ReportanteNombre = "Test",
            Estado = EstadoEmergencia.Despachada
        };
        _context.Emergencias.Add(emergencia);
        _context.SaveChanges();

        foreach (var estado in estados)
        {
            var unidad = new UnidadRespuesta { Tipo = TipoUnidad.Ambulancia, Identificador = Guid.NewGuid().ToString() };
            _context.UnidadesRespuesta.Add(unidad);
            _context.SaveChanges();

            _context.Asignaciones.Add(new Asignacion
            {
                EmergenciaId = emergencia.Id,
                UnidadId = unidad.Id,
                EstadoAsignacion = estado
            });
        }
        _context.SaveChanges();

        return emergencia;
    }

    [Test]
    public async Task ConUnaSolaUnidad_LaEmergenciaSigueElAvanceDeLaAsignacion()
    {
        var emergencia = CrearEmergenciaConAsignaciones(EstadoAsignacion.Despachada);
        var asignacion = _context.Asignaciones.Single();

        await _handler.Handle(new CambiarEstadoAsignacionCommand
        {
            AsignacionId = asignacion.Id,
            NuevoEstado = EstadoAsignacion.EnRuta
        }, CancellationToken.None);

        var actualizada = await _context.Emergencias.FindAsync(emergencia.Id);
        Assert.That(actualizada!.Estado, Is.EqualTo(EstadoEmergencia.EnRuta));
    }

    [Test]
    public async Task ConUnaSolaUnidad_AlLlegarAAtendida_LaEmergenciaPasaAAtendida()
    {
        var emergencia = CrearEmergenciaConAsignaciones(EstadoAsignacion.EnElLugar);
        var asignacion = _context.Asignaciones.Single();

        await _handler.Handle(new CambiarEstadoAsignacionCommand
        {
            AsignacionId = asignacion.Id,
            NuevoEstado = EstadoAsignacion.Atendida
        }, CancellationToken.None);

        var actualizada = await _context.Emergencias.FindAsync(emergencia.Id);
        Assert.That(actualizada!.Estado, Is.EqualTo(EstadoEmergencia.Atendida));
    }

    [Test]
    public async Task ConVariasUnidades_NoPasaAAtendidaHastaQueTodasLleganAAtendida()
    {
        var emergencia = CrearEmergenciaConAsignaciones(EstadoAsignacion.Atendida, EstadoAsignacion.EnElLugar);
        var pendiente = _context.Asignaciones.First(a => a.EstadoAsignacion == EstadoAsignacion.EnElLugar);

        await _handler.Handle(new CambiarEstadoAsignacionCommand
        {
            AsignacionId = pendiente.Id,
            NuevoEstado = EstadoAsignacion.Atendida
        }, CancellationToken.None);

        var actualizada = await _context.Emergencias.FindAsync(emergencia.Id);
        Assert.That(actualizada!.Estado, Is.EqualTo(EstadoEmergencia.Atendida));
    }

    [Test]
    public void NoPermiteRetrocederOMantenerElMismoEstado()
    {
        CrearEmergenciaConAsignaciones(EstadoAsignacion.EnRuta);
        var asignacion = _context.Asignaciones.Single();

        Assert.ThrowsAsync<ConflictException>(async () =>
            await _handler.Handle(new CambiarEstadoAsignacionCommand
            {
                AsignacionId = asignacion.Id,
                NuevoEstado = EstadoAsignacion.Despachada
            }, CancellationToken.None));
    }
}
