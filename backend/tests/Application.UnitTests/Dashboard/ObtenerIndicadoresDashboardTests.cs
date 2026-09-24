using Sige.Application.Dashboard.Queries;
using Sige.Domain.Entities;
using Sige.Domain.Enums;
using Sige.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Sige.Application.UnitTests.Dashboard;

// FR-011: indicadores operativos del dashboard.
public class ObtenerIndicadoresDashboardTests
{
    private ApplicationDbContext _context = null!;
    private ObtenerIndicadoresDashboardQueryHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _handler = new ObtenerIndicadoresDashboardQueryHandler(_context);

        _context.Emergencias.AddRange(
            new Emergencia { Tipo = "Medica", Descripcion = "d1", ReportanteNombre = "r1", Estado = EstadoEmergencia.Reportada, Prioridad = Prioridad.Alta },
            new Emergencia { Tipo = "Incendio", Descripcion = "d2", ReportanteNombre = "r2", Estado = EstadoEmergencia.Despachada, Prioridad = Prioridad.Critica },
            new Emergencia { Tipo = "Medica", Descripcion = "d3", ReportanteNombre = "r3", Estado = EstadoEmergencia.Cerrada, Prioridad = Prioridad.Baja });

        _context.UnidadesRespuesta.AddRange(
            new UnidadRespuesta { Tipo = TipoUnidad.Ambulancia, Identificador = "A1", EstadoOperativo = EstadoOperativoUnidad.Disponible },
            new UnidadRespuesta { Tipo = TipoUnidad.Bomberos, Identificador = "B1", EstadoOperativo = EstadoOperativoUnidad.Ocupada },
            new UnidadRespuesta { Tipo = TipoUnidad.Patrullero, Identificador = "P1", EstadoOperativo = EstadoOperativoUnidad.FueraDeServicio });

        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task ExcluyeEmergenciasCerradasDelConteoPorEstado()
    {
        var resultado = await _handler.Handle(new ObtenerIndicadoresDashboardQuery(), CancellationToken.None);

        Assert.That(resultado.EmergenciasPorEstado.Values.Sum(), Is.EqualTo(2));
        Assert.That(resultado.EmergenciasPorEstado.ContainsKey(nameof(EstadoEmergencia.Cerrada)), Is.False);
    }

    [Test]
    public async Task CuentaUnidadesPorEstadoOperativoCorrectamente()
    {
        var resultado = await _handler.Handle(new ObtenerIndicadoresDashboardQuery(), CancellationToken.None);

        Assert.That(resultado.UnidadesDisponibles, Is.EqualTo(1));
        Assert.That(resultado.UnidadesOcupadas, Is.EqualTo(1));
        Assert.That(resultado.UnidadesFueraDeServicio, Is.EqualTo(1));
    }
}
