using Sige.Application.Recursos.Queries;
using Sige.Domain.Entities;
using Sige.Domain.Enums;
using Sige.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Sige.Application.UnitTests.Recursos;

// FR-112: un recurso con CantidadDisponible < CantidadMinima se marca bajoStock.
public class AlertaBajoStockTests
{
    private ApplicationDbContext _context = null!;
    private ListarRecursosPorUnidadQueryHandler _handler = null!;
    private int _unidadId;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _handler = new ListarRecursosPorUnidadQueryHandler(_context);

        var unidad = new UnidadRespuesta { Tipo = TipoUnidad.Ambulancia, Identificador = "TEST-01" };
        _context.UnidadesRespuesta.Add(unidad);
        _context.SaveChanges();
        _unidadId = unidad.Id;
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task DebeMarcarBajoStock_CuandoDisponibleEsMenorQueMinima()
    {
        _context.Recursos.Add(new Recurso
        {
            Codigo = "AGUA-01", Nombre = "Bidones de agua", Categoria = CategoriaRecurso.Liquidos,
            Cantidad = 50, CantidadDisponible = 3, CantidadMinima = 10, UnidadRespuestaId = _unidadId
        });
        _context.SaveChanges();

        var resultado = await _handler.Handle(new ListarRecursosPorUnidadQuery { UnidadRespuestaId = _unidadId }, CancellationToken.None);

        Assert.That(resultado.Single().BajoStock, Is.True);
    }

    [Test]
    public async Task NoDebeMarcarBajoStock_CuandoDisponibleEsMayorOIgualQueMinima()
    {
        _context.Recursos.Add(new Recurso
        {
            Codigo = "AGUA-02", Nombre = "Bidones de agua", Categoria = CategoriaRecurso.Liquidos,
            Cantidad = 50, CantidadDisponible = 20, CantidadMinima = 10, UnidadRespuestaId = _unidadId
        });
        _context.SaveChanges();

        var resultado = await _handler.Handle(new ListarRecursosPorUnidadQuery { UnidadRespuestaId = _unidadId }, CancellationToken.None);

        Assert.That(resultado.Single().BajoStock, Is.False);
    }
}
